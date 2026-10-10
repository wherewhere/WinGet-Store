using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using WinGetStore.Common;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.ManagerPages
{
    public sealed partial class DownloadsViewModel(CoreDispatcher dispatcher) : ManagerViewModelBase(dispatcher)
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
                MatchResults = [.. packagesResult.Matches.AsReader().Select(x => x.CatalogPackage)];
                WaitProgressText = _loader.GetString("Finished");
                IsLoading = false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<DownloadsViewModel>().LogError(ex, "Failed to refresh downloads page. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
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

                PackageCatalogReference installingSearchCatalogRef = packageManager.GetLocalPackageCatalog(LocalPackageCatalog.InstallingPackages);
                ConnectResult connectResult = await installingSearchCatalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<DownloadsViewModel>().LogError(ex, "Failed to create package catalog. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
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
                SettingsHelper.LoggerFactory.CreateLogger<DownloadsViewModel>().LogError(ex, "Failed to find packages. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                SetError(_loader.GetString("SomethingWrong"), ex.Message, $"0x{ex.HResult:X}");
                return null;
            }
        }
    }
}
