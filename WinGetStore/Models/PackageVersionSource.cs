using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.ViewModels;

namespace WinGetStore.Models
{
    public sealed partial class PackageVersionSource(CatalogPackage catalogPackage, CoreDispatcher dispatcher) : IncrementalLoadingBase<CatalogPackageVersion>(dispatcher)
    {
        private List<PackageVersionId> availableVersions;

        public override bool HasMoreItems => _hasMoreItems;

        /// <summary>
        /// The refresh will clear current items, and re-fetch from beginning, so that we will keep a correct page number.
        /// </summary>
        public async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            await ClearAsync().ConfigureAwait(false);
            _ = await LoadMoreItemsAsync(15);
        }

        public async Task Refresh(bool reset = false)
        {
            if (_busy) { return; }
            if (reset)
            {
                await Reset().ConfigureAwait(false);
            }
            else if (_hasMoreItems)
            {
                _ = await LoadMoreItemsAsync(15);
            }
        }

        protected override async ValueTask<uint> LoadMoreItemsOverrideAsync(uint count, CancellationToken cancellationToken)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();

            if (_currentPage++ == 1)
            {
                try
                {
                    availableVersions = new List<PackageVersionId>(catalogPackage.AvailableVersions.Count);
                    for (int i = 0; i < catalogPackage.AvailableVersions.Count; i++)
                    {
                        availableVersions.Add(catalogPackage.AvailableVersions[i]);
                    }
                }
                catch (Exception ex)
                {
                    SettingsHelper.LoggerFactory.CreateLogger<PackageVersionSource>().LogWarning(ex, "Failed to init package version list. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                }
            }

            List<PackageVersionId> result;
            if (availableVersions.Count > count)
            {
                result = availableVersions.GetRange(0, (int)count);
                availableVersions.RemoveRange(0, (int)count);
            }
            else
            {
                result = availableVersions;
                _hasMoreItems = false;
            }

            string currents = this.LastOrDefault()?.Version;
            uint loaded = 0;
            foreach (PackageVersionId item in result)
            {
                if (currents != item.Version)
                {
                    PackageVersionInfo versionInfo = catalogPackage.GetPackageVersionInfo(item);
                    CatalogPackageMetadata packageMetadata = versionInfo.GetCatalogPackageMetadata();
                    await AddAsync(new(versionInfo.Version, packageMetadata));
                    currents = item.Version;
                    loaded++;
                }
            }
            return loaded;
        }

        private int _currentPage = 1;
        private bool _hasMoreItems = true;
    }
}
