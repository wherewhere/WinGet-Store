﻿using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using WinGetStore.Common;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public partial class SearchingViewModel(string keyword, CoreDispatcher dispatcher) : INotifyPropertyChanged
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("MainPage");

        public CoreDispatcher Dispatcher => dispatcher;
        public string Title
        {
            get => keyword;
            set => SetProperty(ref keyword, value);
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private string waitProgressText = _loader.GetString("Searching");
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

        private IList<PackageMatchFilter> selectors = [];
        public IList<PackageMatchFilter> Selectors
        {
            get => selectors;
            set => SetProperty(ref selectors, value);
        }

        private IList<PackageMatchFilter> filters = [];
        public IList<PackageMatchFilter> Filters
        {
            get => filters;
            set => SetProperty(ref filters, value);
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

                WaitProgressText = _loader.GetString("Searching");
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
                FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog, keyword);
                if (packagesResult is null)
                {
                    SetError(_loader.GetString("GettingResultsFailedTitle"), _loader.GetString("GettingResultsFailedDescription"));
                    return;
                }

                WaitProgressText = _loader.GetString("ProcessingResults");
                MatchResults = [.. packagesResult.Matches.AsReader().Select(x => x.CatalogPackage)];
                WaitProgressText = _loader.GetString("Finished");
                IsLoading = false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<SearchingViewModel>().LogError(ex, "Failed to refresh search page. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
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

                IReadOnlyList<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs();
                if (packageCatalogReferences?.Count is not > 0)
                {
                    SetError(_loader.GetString("NoCatalogTitle"), _loader.GetString("NoCatalogDescription"));
                    return null;
                }

                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.AddRange(packageCatalogReferences.AsReader());

                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<SearchingViewModel>().LogError(ex, "Failed to create package catalog. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
                return null;
            }
        }

        private async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog, string packageId)
        {
            try
            {
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();

                if (selectors?.Count > 0 || filters?.Count > 0)
                {
                    findPackagesOptions.Selectors.AddRange(selectors ?? []);
                    findPackagesOptions.Filters.AddRange(filters ?? []);
                }
                else
                {
                    PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
                    filter.Field = PackageMatchField.Id;
                    filter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
                    filter.Value = packageId;
                    findPackagesOptions.Selectors.Add(filter);
                    List<PackageMatchFilter> selectors = [.. findPackagesOptions.Selectors];
                    Selectors = selectors;
                }

                return await catalog.FindPackagesAsync(findPackagesOptions);
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<SearchingViewModel>().LogError(ex, "Failed to find packages. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
                return null;
            }
        }
    }
}
