using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using WinGetStore.Common;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public sealed partial class SearchingViewModel(string keyword, CoreDispatcher dispatcher) : ManagerViewModelBase(dispatcher)
    {
        public string Title { get => keyword; set => SetProperty(ref keyword, value); }
        public IList<PackageMatchFilter> Selectors { get; set => SetProperty(ref field, value); } = [];
        public IList<PackageMatchFilter> Filters { get; set => SetProperty(ref field, value); } = [];

        public override async Task Refresh(bool reset = true)
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

        private async ValueTask<PackageCatalog> CreatePackageCatalogAsync()
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

        private async ValueTask<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog, string packageId)
        {
            try
            {
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();

                if (Selectors?.Count > 0 || Filters?.Count > 0)
                {
                    findPackagesOptions.Selectors.AddRange(Selectors ?? []);
                    findPackagesOptions.Filters.AddRange(Filters ?? []);
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
