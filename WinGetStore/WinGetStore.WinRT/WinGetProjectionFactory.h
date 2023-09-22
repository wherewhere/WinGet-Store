#pragma once

#include "WinGetProjectionFactory.g.h"
#include "winrt/Microsoft.Management.Deployment.h"

using namespace winrt::Microsoft::Management::Deployment;

namespace winrt::WinGetStore::WinRT::implementation
{
    struct WinGetProjectionFactory : WinGetProjectionFactoryT<WinGetProjectionFactory>
    {
        WinGetProjectionFactory() = default;

        static bool IsUseDev() { return useDev; }
        static void IsUseDev(bool value) { useDev = value; }

        static bool IsWinGetInstalled();
        static bool IsWinGetDevInstalled();

        static PackageManager CreatePackageManager();
        static InstallOptions CreateInstallOptions();
        static DownloadOptions CreateDownloadOptions();
        static UninstallOptions CreateUninstallOptions();
        static FindPackagesOptions CreateFindPackagesOptions();
        static CreateCompositePackageCatalogOptions CreateCreateCompositePackageCatalogOptions();
        static PackageMatchFilter CreatePackageMatchFilter();
        static PackageManagerSettings CreatePackageManagerSettings();

        static PackageManager TryCreatePackageManager();
        static InstallOptions TryCreateInstallOptions();
        static DownloadOptions TryCreateDownloadOptions();
        static UninstallOptions TryCreateUninstallOptions();
        static FindPackagesOptions TryCreateFindPackagesOptions();
        static CreateCompositePackageCatalogOptions TryCreateCreateCompositePackageCatalogOptions();
        static PackageMatchFilter TryCreatePackageMatchFilter();
        static PackageManagerSettings TryCreatePackageManagerSettings();

    private:
        static bool useDev;
    };
}

namespace winrt::WinGetStore::WinRT::factory_implementation
{
    struct WinGetProjectionFactory : WinGetProjectionFactoryT<WinGetProjectionFactory, implementation::WinGetProjectionFactory>
    {
    };
}
