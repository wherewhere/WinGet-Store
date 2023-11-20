using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.ViewModels;

namespace WinGetStore.Models
{
    public class PackageVersionSource(CatalogPackage catalogPackage) : IncrementalLoadingBase<CatalogPackageVersion, PackageVersionId>
    {
        private List<PackageVersionId> availableVersions;

        /// <summary>
        /// The refresh will clear current items, and re-fetch from beginning, so that we will keep a correct page number.
        /// </summary>
        public virtual async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            await ClearAsync().ConfigureAwait(false);
            _ = await LoadMoreItemsAsync(15);
        }

        public virtual async Task Refresh(bool reset = false)
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

        protected override async Task<ICollection<PackageVersionId>> LoadMoreItemsOverrideAsync(CancellationToken cancellationToken, uint count)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();

            if (_currentPage++ == 1)
            {
                try
                {
                    availableVersions = catalogPackage.AvailableVersions?.ToList();
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(PackageVersionSource)).Warn(ex.ExceptionToMessage());
                }
            }

            if (availableVersions.Count > count)
            {
                List<PackageVersionId> result = availableVersions.GetRange(0, (int)count);
                availableVersions.RemoveRange(0, (int)count);
                return result;
            }
            else
            {
                List<PackageVersionId> result = availableVersions;
                _hasMoreItems = false;
                return result;
            }
        }

        protected override async Task AddItemsAsync(ICollection<PackageVersionId> items)
        {
            string currents = this.LastOrDefault()?.Version;
            foreach (PackageVersionId item in items)
            {
                if (currents != item.Version)
                {
                    PackageVersionInfo versionInfo = catalogPackage.GetPackageVersionInfo(item);
                    CatalogPackageMetadata packageMetadata = versionInfo.GetCatalogPackageMetadata();
                    await AddAsync(new(versionInfo.Version, packageMetadata));
                    currents = item.Version;
                }
            }
        }

        protected override bool HasMoreItemsOverride() => _hasMoreItems;

        protected int _currentPage = 1;
        protected bool _hasMoreItems = true;
    }
}
