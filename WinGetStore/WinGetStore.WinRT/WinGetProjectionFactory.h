#pragma once

#include "WinGetProjectionFactory.g.h"
#include "winrt/Microsoft.Management.Deployment.h"

using namespace winrt::Microsoft::Management::Deployment;

namespace winrt::WinGetStore::WinRT::implementation
{
    struct WinGetProjectionFactory : WinGetProjectionFactoryT<WinGetProjectionFactory>
    {
        static WinGetStore::WinRT::WinGetProjectionFactory WinGetProjectionFactory::Instance();

        WinGetProjectionFactory() = default;

        PackageManager CreatePackageManager(bool useDev);
        InstallOptions CreateInstallOptions(bool useDev);
        UninstallOptions CreateUninstallOptions(bool useDev);
        FindPackagesOptions CreateFindPackagesOptions(bool useDev);
        CreateCompositePackageCatalogOptions CreateCreateCompositePackageCatalogOptions(bool useDev);
        PackageMatchFilter CreatePackageMatchFilter(bool useDev);
        PackageManagerSettings CreatePackageManagerSettings();

    private:
        static WinGetStore::WinRT::WinGetProjectionFactory instance;
    };
}

namespace winrt::WinGetStore::WinRT::factory_implementation
{
    struct WinGetProjectionFactory : WinGetProjectionFactoryT<WinGetProjectionFactory, implementation::WinGetProjectionFactory>
    {
    };
}
