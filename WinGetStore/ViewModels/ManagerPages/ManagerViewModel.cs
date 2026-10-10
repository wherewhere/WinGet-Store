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
    public sealed partial class ManagerViewModel(CoreDispatcher dispatcher) : ManagerViewModelBase(dispatcher)
    {
        public override async Task Refresh(bool reset = true)
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
                MatchResults =
                    [.. packagesResult.Matches.AsReader()
                                              .Where(x => x.CatalogPackage.AvailableVersions is { Count: > 0 })
                                              .OrderByDescending(item => item.CatalogPackage.IsUpdateAvailable)
                                              .Select(x => x.CatalogPackage)];
                WaitProgressText = _loader.GetString("Finished");
                IsLoading = false;

                try
                {
                    await UpdateTileAsync();
                }
                catch (Exception ex)
                {
                    SettingsHelper.LoggerFactory.CreateLogger<ManagerViewModel>().LogError(ex, "Failed to update tile. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManagerViewModel>().LogError(ex, "Failed to refresh manager page. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
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
                createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;

                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManagerViewModel>().LogError(ex, "Failed to create package catalog. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
                return null;
            }
        }

        private async ValueTask<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog)
        {
            try
            {
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
                return await catalog.FindPackagesAsync(findPackagesOptions);
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManagerViewModel>().LogError(ex, "Failed to find packages. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
                return null;
            }
        }

        private async Task UpdateTileAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            CatalogPackage[] available = [.. matchResults.Where(x => x.IsUpdateAvailable)];
            TilesHelper.SetBadgeNumber((uint)available.Length);
            available.Take(5)
                     .Select(TilesHelper.CreateTile)
                     .UpdateTiles();
        }
    }
}
