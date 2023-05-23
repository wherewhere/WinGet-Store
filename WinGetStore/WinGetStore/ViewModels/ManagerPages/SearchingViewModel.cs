using AppInstallerCaller;
using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp;
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
            set => SetProperty(ref title, value);
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string waitProgressText = "Searching...";
        public string WaitProgressText
        {
            get => waitProgressText;
            set => SetProperty(ref waitProgressText, value);
        }

        private bool isError = false;
        public bool IsError
        {
            get => isError;
            set => SetProperty(ref isError, value);
        }

        private string errorDescription;
        public string ErrorDescription
        {
            get => errorDescription;
            set => SetProperty(ref errorDescription, value);
        }

        private string errorLongDescription;
        public string ErrorLongDescription
        {
            get => errorLongDescription;
            set => SetProperty(ref errorLongDescription, value);
        }

        private string errorCode;
        public string ErrorCode
        {
            get => errorCode;
            set => SetProperty(ref errorCode, value);
        }

        private ObservableCollection<CatalogPackage> matchResults = new();
        public ObservableCollection<CatalogPackage> MatchResults
        {
            get => matchResults;
            set => SetProperty(ref matchResults, value);
        }

        public IList<PackageMatchFilter> PackageMatchFilters { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        public SearchingViewModel(string keyword) => Title = keyword;

        private void SetError(string title, string description, string code = "")
        {
            if (IsError) { return; }
            IsError = true;
            IsLoading = false;
            ErrorDescription = title;
            ErrorLongDescription = description;
            ErrorCode = code;
        }

        private void RemoveError()
        {
            IsError = false;
            ErrorDescription = string.Empty;
            ErrorLongDescription = string.Empty;
            ErrorCode = string.Empty;
        }

        public async Task Refresh()
        {
            try
            {
                if (IsLoading) { return; }

                WaitProgressText = "Searching...";
                IsLoading = true;

                RemoveError();
                MatchResults.Clear();

                await ThreadSwitcher.ResumeBackgroundAsync();
                WaitProgressText = "Connect to WinGet...";
                PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
                if (packageCatalog is null)
                {
                    SetError("Please check your connection", "Fail to connect to WinGet. It seems you are not connect to network.");
                    return;
                }

                WaitProgressText = "Getting results...";
                FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog, title);
                if (packagesResult is null)
                {
                    SetError("Fail to get result", "There are something wrong with WinGet.");
                    return;
                }

                WaitProgressText = "Processing results...";
                await Dispatcher.ResumeForegroundAsync();
                packagesResult.Matches.ToList().ForEach((x) => MatchResults.Add(x.CatalogPackage));
                WaitProgressText = "Finnish";
                IsLoading = false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SearchingViewModel)).Error(ex.ExceptionToMessage());
                SetError("Something is wrong.", ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
                return;
            }
        }

        private async Task<PackageCatalog> CreatePackageCatalogAsync()
        {
            try
            {
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                if (packageManager is null)
                {
                    SetError("WinGet is not installed", "Cannot connect to WinGet. It seems that you are not installed WinGet or the installed version is out of date.");
                    return null;
                }

                List<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs()?.ToList();
                if (packageCatalogReferences is null || !packageCatalogReferences.Any())
                {
                    SetError("There are no catalog set in WinGet", "Please make sure you have opened WinGet in console and agreed the license, or you have not delete all catalog in WinGet.");
                    return null;
                }

                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                {
                    createCompositePackageCatalogOptions.Catalogs.Add(catalogReference);
                }
                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SearchingViewModel)).Error(ex.ExceptionToMessage());
                SetError("Something is wrong.", ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
                return null;
            }
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog, string packageId)
        {
            try
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
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SearchingViewModel)).Error(ex.ExceptionToMessage());
                SetError("Something is wrong.", ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
                return null;
            }
        }

        public bool IsEqual(SearchingViewModel other) => other is SearchingViewModel model && IsEqual(model);
    }
}
