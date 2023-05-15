using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public class SearchingViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

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

        public IList<PackageMatchFilter> PackageMatchFilters { get; set; }

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

        public SearchingViewModel(string keyword) => Title = keyword;

        public async Task Refresh()
        {
            if (IsLoading) { return; }
            WaitProgressText = "Searching...";
            IsLoading = true;
            MatchResults.Clear();
            await ThreadSwitcher.ResumeBackgroundAsync();
            WaitProgressText = "Connect to WinGet...";
            PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
            WaitProgressText = "Getting results...";
            FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog, title);
            WaitProgressText = "Processing results...";
            await Dispatcher.ResumeForegroundAsync();
            packagesResult.Matches.ToList().ForEach((x) => MatchResults.Add(x.CatalogPackage));
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
            PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
            ConnectResult connectResult = await catalogRef.ConnectAsync();
            return connectResult.PackageCatalog;
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog, string packageId)
        {
            FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();

            bool isEmpty = true;
            if (PackageMatchFilters != null)
            {
                List<PackageMatchFilter> filters = PackageMatchFilters.ToList();
                if (filters.Any())
                {
                    filters.ForEach(findPackagesOptions.Filters.Add);
                    isEmpty = false;
                }
            }

            if (isEmpty)
            {
                PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
                filter.Field = PackageMatchField.Id;
                filter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
                filter.Value = packageId;
                findPackagesOptions.Filters.Add(filter);
                PackageMatchFilters = findPackagesOptions.Filters;
            }

            return await catalog.FindPackagesAsync(findPackagesOptions);
        }

        public bool IsEqual(SearchingViewModel other) => other is SearchingViewModel model && IsEqual(model);
    }
}
