using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using WinGetStore.Common;

namespace WinGetStore.Helpers
{
    public static class PackageHelper
    {
        public static async Task<IEnumerable<Package>> FindPackagesByNameAsync(string PackageName)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageManager manager = new();
            try
            {
                IEnumerable<Package> packages = manager.FindPackagesForUser("");
                IEnumerable<Package> results = packages?.Where(x => x.Id.FamilyName.StartsWith(PackageName));
                return results ?? [];
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(PackageHelper)).LogWarning(ex, "Failed to find packages. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                return [];
            }
        }

        public static async Task<Package> FindPackagesByFamilyNameAsync(string PackageFamilyName)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageManager manager = new();
            try
            {
                return manager.FindPackageForUser("", PackageFamilyName);
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(PackageHelper)).LogWarning(ex, "Failed to find package. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                return null;
            }
        }
    }
}
