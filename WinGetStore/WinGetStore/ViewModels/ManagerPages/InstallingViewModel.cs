using Microsoft.Management.Deployment;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.WinRT;

namespace WinGetStore.ViewModels.ManagerPages
{
    public class InstallingViewModel : INotifyPropertyChanged
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("MainPage");

        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string waitProgressText = _loader.GetString("Loading");
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

        private ObservableCollection<CatalogPackage> matchResults = [];
        public ObservableCollection<CatalogPackage> MatchResults
        {
            get => matchResults;
            set => SetProperty(ref matchResults, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public InstallingViewModel() => waitProgressText = _loader.GetString("Loading");

        private async void SetError(string title, string description, string code = "")
        {
            if (isError) { return; }
            IsError = true;
            IsLoading = false;
            ErrorDescription = title;
            ErrorLongDescription = description;
            ErrorCode = code;
            await Dispatcher.ResumeForegroundAsync();
            matchResults.Clear();
        }

        private void RemoveError()
        {
            if (!isError) { return; }
            IsError = false;
            ErrorDescription = string.Empty;
            ErrorLongDescription = string.Empty;
            ErrorCode = string.Empty;
        }

        public async Task Refresh()
        {
            try
            {
                if (isLoading) { return; }

                WaitProgressText = _loader.GetString("Loading");
                IsLoading = true;

                RemoveError();
                matchResults.Clear();

                await ThreadSwitcher.ResumeBackgroundAsync();
                WaitProgressText = _loader.GetString("ConnectingWinGet");
                PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
                if (packageCatalog is null)
                {
                    SetError(_loader.GetString("ConnectWinGetFailedTitle"), _loader.GetString("ConnectWinGetFailedDescription"));
                    return;
                }

                WaitProgressText = _loader.GetString("GettingResults");
                FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
                if (packagesResult is null)
                {
                    SetError(_loader.GetString("GettingResultsFailedTitle"), _loader.GetString("GettingResultsFailedDescription"));
                    return;
                }

                WaitProgressText = _loader.GetString("ProcessingResults");
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                PackageCatalogReference[] packageCatalogReferences = packageManager.GetPackageCatalogs()?.ToArray();
                packagesResult.Matches.ToArray()
                    .ForEach(async (x) =>
                    {
                        foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                        {
                            try
                            {
                                IAsyncOperationWithProgress<InstallResult, InstallProgress> installOperation = packageManager.GetInstallProgress(x.CatalogPackage, catalogReference.Info);
                                if (installOperation != null)
                                {
                                    CatalogPackage package = await GetPackageByID(x.CatalogPackage.Id);
                                    await Dispatcher.ResumeForegroundAsync();
                                    matchResults.Add(package ?? x.CatalogPackage);
                                    if (isLoading) { IsLoading = false; }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                SettingsHelper.LogManager.GetLogger(nameof(InstallingViewModel)).Warn(ex.ExceptionToMessage());
                            }
                        }
                    });
                WaitProgressText = _loader.GetString("Finished");
                IsLoading = false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallingViewModel)).Error(ex.ExceptionToMessage());
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
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
                    SetError(_loader.GetString("WinGetNotInstalledTitle"), _loader.GetString("WinGetNotInstalledDescription"));
                    return null;
                }

                PackageCatalogReference installingSearchCatalogRef = packageManager.GetLocalPackageCatalog(LocalPackageCatalog.InstallingPackages);
                ConnectResult connectResult = await installingSearchCatalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallingViewModel)).Error(ex.ExceptionToMessage());
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
                return null;
            }
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog)
        {
            try
            {
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
                return await catalog.FindPackagesAsync(findPackagesOptions);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallingViewModel)).Error(ex.ExceptionToMessage());
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
                return null;
            }
        }

        private async Task<CatalogPackage> GetPackageByID(string packageID)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                PackageCatalogReference[] packageCatalogReferences = packageManager.GetPackageCatalogs()?.ToArray();
                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.AddRange(packageCatalogReferences);
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
                return packagesResult.Matches.ToArray().FirstOrDefault()?.CatalogPackage;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(InstallingViewModel)).Error(ex.ExceptionToMessage());
                return null;
            }
        }
    }
}
