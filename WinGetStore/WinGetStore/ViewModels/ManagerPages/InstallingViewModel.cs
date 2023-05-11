using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public class InstallingViewModel : INotifyPropertyChanged
    {
        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string waitProgressText = "Searching...";
        public string WaitProgressText
        {
            get => waitProgressText;
            set
            {
                if (waitProgressText != value)
                {
                    waitProgressText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ObservableCollection<CatalogPackage> matchResults = new();
        public ObservableCollection<CatalogPackage> MatchResults
        {
            get => matchResults;
            set
            {
                if (matchResults != value)
                {
                    matchResults = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (!Dispatcher.HasThreadAccess)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public async Task Refresh()
        {
            WaitProgressText = "Searching...";
            IsLoading = true;
            MatchResults.Clear();
            await ThreadSwitcher.ResumeBackgroundAsync();
            WaitProgressText = "Connect to WinGet...";
            PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
            WaitProgressText = "Getting results...";
            FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
            WaitProgressText = "Processing results...";
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            List<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs().ToList();
            packagesResult.Matches.ToList()
                .ForEach(async (x) =>
                {
                    foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                    {
                        IAsyncOperationWithProgress<InstallResult, InstallProgress> installOperation = packageManager.GetInstallProgress(x.CatalogPackage, catalogReference.Info);
                        if (installOperation != null)
                        {
                            CatalogPackage package = await GetPackageByID(x.CatalogPackage.Id);
                            await Dispatcher.ResumeForegroundAsync();
                            MatchResults.Add(package ?? x.CatalogPackage);
                            break;
                        }
                    }
                });
            WaitProgressText = "Finnish";
            IsLoading = false;
        }

        private async Task<PackageCatalog> CreatePackageCatalogAsync()
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            PackageCatalogReference installingSearchCatalogRef = packageManager.GetLocalPackageCatalog(LocalPackageCatalog.InstallingPackages);
            ConnectResult connectResult = await installingSearchCatalogRef.ConnectAsync();
            return connectResult.PackageCatalog;
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog)
        {
            FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
            return await catalog.FindPackagesAsync(findPackagesOptions);
        }

        private async Task<CatalogPackage> GetPackageByID(string packageID)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            List<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs().ToList();
            CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
            foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
            {
                createCompositePackageCatalogOptions.Catalogs.Add(catalogReference);
            }
            PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
            ConnectResult connectResult = await catalogRef.ConnectAsync();
            PackageCatalog catalog = connectResult.PackageCatalog;
            FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
            PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
            filter.Field = PackageMatchField.Id;
            filter.Option = PackageFieldMatchOption.Equals;
            filter.Value = packageID;
            findPackagesOptions.Filters.Add(filter);
            FindPackagesResult packagesResult = await catalog.FindPackagesAsync(findPackagesOptions);
            return packagesResult.Matches.ToList().FirstOrDefault()?.CatalogPackage;
        }
    }
}
