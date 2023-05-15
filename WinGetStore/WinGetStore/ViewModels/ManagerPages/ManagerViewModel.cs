using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public class ManagerViewModel : INotifyPropertyChanged
    {
        private bool isLoading;
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

        private bool isError;
        public bool IsError
        {
            get => isError;
            set => SetProperty(ref isError, value);
        }

        private string waitProgressText = "Loading...";
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


        private string errorText = "Loading...";
        public string ErrorText
        {
            get => errorText;
            set => SetProperty(ref errorText, value);
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

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property.Equals(value)) { return; }
            property = value;
            Dispatcher.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }

        private void SetError(string errorText)
        {
            IsError = true;
            IsLoading = false;
            ErrorText = errorText;
        }

        public async Task Refresh()
        {
            if (IsLoading) { return; }
            IsError = false;
            ErrorText = string.Empty;
            WaitProgressText = "Loading...";
            IsLoading = true;
            MatchResults.Clear();

            await ThreadSwitcher.ResumeBackgroundAsync();
            WaitProgressText = "Connect to WinGet...";
            PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
            if (packageCatalog is null)
            {
                SetError("Fail to connect to WinGet.");
                return;
            }

            WaitProgressText = "Getting results...";
            FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
            if (packagesResult is null)
            {
                SetError("Fail to get result.");
                return;
            }

            WaitProgressText = "Processing results...";
            await Dispatcher.ResumeForegroundAsync();
            packagesResult.Matches.ToList()
                .OrderByDescending(item => item.CatalogPackage.IsUpdateAvailable)
                .ToList().ForEach((x) =>
                {
                    if (x.CatalogPackage.DefaultInstallVersion != null)
                    {
                        MatchResults.Add(x.CatalogPackage);
                    }
                });
            WaitProgressText = "Finnish";
            IsLoading = false;
        }

        private async Task<PackageCatalog> CreatePackageCatalogAsync()
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            List<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs().ToList();
            CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
            foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
            {
                createCompositePackageCatalogOptions.Catalogs.Add(catalogReference);
            }
            createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;
            PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
            ConnectResult connectResult = await catalogRef.ConnectAsync();
            return connectResult.PackageCatalog;
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog)
        {
            FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
            return await catalog.FindPackagesAsync(findPackagesOptions);
        }
    }
}
