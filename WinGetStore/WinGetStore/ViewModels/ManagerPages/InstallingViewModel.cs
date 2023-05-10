using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private ObservableCollection<MatchResult> matchResults = new();
        public ObservableCollection<MatchResult> MatchResults
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

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public async Task Refresh()
        {
            IsLoading = true;
            MatchResults.Clear();
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
            FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            List<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs().ToList();
            await Dispatcher.ResumeForegroundAsync();
            packagesResult.Matches.ToList()
                .ForEach((x) =>
                {
                    foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                    {
                        IAsyncOperationWithProgress<InstallResult, InstallProgress> installOperation = packageManager.GetInstallProgress(x.CatalogPackage, catalogReference.Info);
                        if (installOperation != null)
                        {
                            MatchResults.Add(x);
                            break;
                        }
                    }
                });
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
    }
}
