﻿using System;
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
        public static async Task<IEnumerable<Package>> FindPackagesByName(string PackageName)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageManager manager = new();
            try
            {
                IEnumerable<Package> packages = manager.FindPackagesForUser("");
                IEnumerable<Package> results = packages?.Where((x) => x.Id.FamilyName.StartsWith(PackageName));
                return results ?? Array.Empty<Package>();
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(PackageHelper)).Warn(ex.ExceptionToMessage());
                return Array.Empty<Package>();
            }
        }

        public static async Task<Package> FindPackagesByFamilyName(string PackageFamilyName)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            PackageManager manager = new();
            try
            {
                return manager.FindPackageForUser("", PackageFamilyName);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(PackageHelper)).Warn(ex.ExceptionToMessage());
                return null;
            }
        }
    }
}
