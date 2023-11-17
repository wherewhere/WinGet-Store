#pragma once

#include "WinGetProjectionFactory.g.h"
#include "winrt/Microsoft.Management.Configuration.h"
#include "winrt/Microsoft.Management.Deployment.h"

using namespace winrt::Microsoft::Management::Configuration;
using namespace winrt::Microsoft::Management::Deployment;

namespace winrt::WinGetStore::WinRT::implementation
{
    struct WinGetProjectionFactory : WinGetProjectionFactoryT<WinGetProjectionFactory>
    {
        static bool IsUseDev() { return useDev; }
        static void IsUseDev(bool value) { useDev = value; }

        static bool IsWinGetInstalled();
        static bool IsWinGetDevInstalled();

        static PackageManager CreatePackageManager();
        static FindPackagesOptions CreateFindPackagesOptions();
        static CreateCompositePackageCatalogOptions CreateCreateCompositePackageCatalogOptions();
        static InstallOptions CreateInstallOptions();
        static UninstallOptions CreateUninstallOptions();
        static PackageMatchFilter CreatePackageMatchFilter();
        static ConfigurationStaticFunctions CreateConfigurationStaticFunctions();
        static DownloadOptions CreateDownloadOptions();
        static PackageManagerSettings CreatePackageManagerSettings();

        static PackageManager TryCreatePackageManager();
        static FindPackagesOptions TryCreateFindPackagesOptions();
        static CreateCompositePackageCatalogOptions TryCreateCreateCompositePackageCatalogOptions();
        static InstallOptions TryCreateInstallOptions();
        static UninstallOptions TryCreateUninstallOptions();
        static PackageMatchFilter TryCreatePackageMatchFilter();
        static ConfigurationStaticFunctions TryCreateConfigurationStaticFunctions();
        static DownloadOptions TryCreateDownloadOptions();
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
