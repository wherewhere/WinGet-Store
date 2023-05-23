using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp;
using Windows.System;
using WinGetStore.Helpers;
using WinGetStore.Common;

namespace WinGetStore.ViewModels.ManagerPages
{
    public class ManagerViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string waitProgressText = "Loading...";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

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

                WaitProgressText = "Loading...";
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
                FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
                if (packagesResult is null)
                {
                    SetError("Fail to get result", "There are something wrong with WinGet.");
                    return;
                }

                WaitProgressText = "Processing results...";
                await Dispatcher.ResumeForegroundAsync();
                packagesResult.Matches.ToList()
                    .OrderByDescending(item => item.CatalogPackage.IsUpdateAvailable)
                    .ForEach((x) =>
                    {
                        if (x.CatalogPackage.DefaultInstallVersion != null)
                        {
                            MatchResults.Add(x.CatalogPackage);
                        }
                    });
                WaitProgressText = "Finnish";
                IsLoading = false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
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
                createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;
                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
                SetError("Something is wrong.", ex.Message, $"0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()}");
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
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
                SetError("Something is wrong.", ex.Message, $"0x{Convert.ToString(ex.HResult, 16)}");
                return null;
            }
        }
    }
}
