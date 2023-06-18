#include "pch.h"
#include "WinGetProjectionFactory.h"
#include "WinGetProjectionFactory.g.cpp"

namespace winrt::WinGetStore::WinRT::implementation
{
    // CLSIDs for WinGet package
    const CLSID CLSID_PackageManager = { 0xC53A4F16, 0x787E, 0x42A4, { 0xB3, 0x04, 0x29, 0xEF, 0xFB, 0x4B, 0xF5, 0x97 } };  //C53A4F16-787E-42A4-B304-29EFFB4BF597
    const CLSID CLSID_InstallOptions = { 0x1095f097, 0xEB96, 0x453B, { 0xB4, 0xE6, 0x16, 0x13, 0x63, 0x7F, 0x3B, 0x14 } };  //1095F097-EB96-453B-B4E6-1613637F3B14
    const CLSID CLSID_UninstallOptions = { 0xE1D9A11E, 0x9F85, 0x4D87, { 0x9C, 0x17, 0x2B, 0x93, 0x14, 0x3A, 0xDB, 0x8D } };  //E1D9A11E-9F85-4D87-9C17-2B93143ADB8D
    const CLSID CLSID_FindPackagesOptions = { 0x572DED96, 0x9C60, 0x4526, { 0x8F, 0x92, 0xEE, 0x7D, 0x91, 0xD3, 0x8C, 0x1A } };  //572DED96-9C60-4526-8F92-EE7D91D38C1A
    const CLSID CLSID_PackageMatchFilter = { 0xD02C9DAF, 0x99DC, 0x429C, { 0xB5, 0x03, 0x4E, 0x50, 0x4E, 0x4A, 0xB0, 0x00 } };  //D02C9DAF-99DC-429C-B503-4E504E4AB000
    const CLSID CLSID_CreateCompositePackageCatalogOptions = { 0x526534B8, 0x7E46, 0x47C8, { 0x84, 0x16, 0xB1, 0x68, 0x5C, 0x32, 0x7D, 0x37 } };  //526534B8-7E46-47C8-8416-B1685C327D37

    const CLSID CLSID_PackageManagerSettings = { 0x80CF9D63, 0x5505, 0x4342, { 0xB9, 0xB4, 0xBB, 0x87, 0x89, 0x5C, 0xA8, 0xBB } };  //80CF9D63-5505-4342-B9B4-BB87895CA8BB

    // CLSIDs for WinGetDev package
    const CLSID CLSID_PackageManager2 = { 0x74CB3139, 0xB7C5, 0x4B9E, { 0x93, 0x88, 0xE6, 0x61, 0x6D, 0xEA, 0x28, 0x8C } };  //74CB3139-B7C5-4B9E-9388-E6616DEA288C
    const CLSID CLSID_InstallOptions2 = { 0x44FE0580, 0x62F7, 0x44D4, { 0x9E, 0x91, 0xAA, 0x96, 0x14, 0xAB, 0x3E, 0x86 } };  //44FE0580-62F7-44D4-9E91-AA9614AB3E86
    const CLSID CLSID_UninstallOptions2 = { 0xAA2A5C04, 0x1AD9, 0x46C4, { 0xB7, 0x4F, 0x6B, 0x33, 0x4A, 0xD7, 0xEB, 0x8C } };  //AA2A5C04-1AD9-46C4-B74F-6B334AD7EB8C
    const CLSID CLSID_FindPackagesOptions2 = { 0x1BD8FF3A, 0xEC50, 0x4F69, { 0xAE, 0xEE, 0xDF, 0x4C, 0x9D, 0x3B, 0xAA, 0x96 } };  //1BD8FF3A-EC50-4F69-AEEE-DF4C9D3BAA96
    const CLSID CLSID_PackageMatchFilter2 = { 0x3F85B9F4, 0x487A, 0x4C48, { 0x90, 0x35, 0x29, 0x03, 0xF8, 0xA6, 0xD9, 0xE8 } };  //3F85B9F4-487A-4C48-9035-2903F8A6D9E8
    const CLSID CLSID_CreateCompositePackageCatalogOptions2 = { 0xEE160901, 0xB317, 0x4EA7, { 0x9C, 0xC6, 0x53, 0x55, 0xC6, 0xD7, 0xD8, 0xA7 } };  //EE160901-B317-4EA7-9CC6-5355C6D7D8A7

    bool WinGetProjectionFactory::useDev = false;

    bool WinGetProjectionFactory::IsWinGetInstalled()
    {
        void* result = nullptr;
        int32_t hresult = WINRT_IMPL_CoCreateInstance(CLSID_PackageManager, nullptr, CLSCTX_ALL, guid_of<PackageManager>(), &result);
        return hresult == S_OK && result != nullptr;
    }

    bool WinGetProjectionFactory::IsWinGetDevInstalled()
    {
        void* result = nullptr;
        int32_t hresult = WINRT_IMPL_CoCreateInstance(CLSID_PackageManager2, nullptr, CLSCTX_ALL, guid_of<PackageManager>(), &result);
        return hresult == S_OK && result != nullptr;
    }

    PackageManager WinGetProjectionFactory::CreatePackageManager()
    {
        if (useDev)
        {
            return winrt::create_instance<PackageManager>(CLSID_PackageManager2, CLSCTX_ALL);
        }
        return winrt::create_instance<PackageManager>(CLSID_PackageManager, CLSCTX_ALL);
    }

    InstallOptions WinGetProjectionFactory::CreateInstallOptions()
    {
        if (useDev)
        {
            return winrt::create_instance<InstallOptions>(CLSID_InstallOptions2, CLSCTX_ALL);
        }
        return winrt::create_instance<InstallOptions>(CLSID_InstallOptions, CLSCTX_ALL);
    }

    UninstallOptions WinGetProjectionFactory::CreateUninstallOptions()
    {
        if (useDev)
        {
            return winrt::create_instance<UninstallOptions>(CLSID_UninstallOptions2, CLSCTX_ALL);
        }
        return winrt::create_instance<UninstallOptions>(CLSID_UninstallOptions, CLSCTX_ALL);
    }

    FindPackagesOptions WinGetProjectionFactory::CreateFindPackagesOptions()
    {
        if (useDev)
        {
            return winrt::create_instance<FindPackagesOptions>(CLSID_FindPackagesOptions2, CLSCTX_ALL);
        }
        return winrt::create_instance<FindPackagesOptions>(CLSID_FindPackagesOptions, CLSCTX_ALL);
    }

    CreateCompositePackageCatalogOptions WinGetProjectionFactory::CreateCreateCompositePackageCatalogOptions()
    {
        if (useDev)
        {
            return winrt::create_instance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions2, CLSCTX_ALL);
        }
        return winrt::create_instance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions, CLSCTX_ALL);
    }

    PackageMatchFilter WinGetProjectionFactory::CreatePackageMatchFilter()
    {
        if (useDev)
        {
            return winrt::create_instance<PackageMatchFilter>(CLSID_PackageMatchFilter2, CLSCTX_ALL);
        }
        return winrt::create_instance<PackageMatchFilter>(CLSID_PackageMatchFilter, CLSCTX_ALL);
    }

    PackageManagerSettings WinGetProjectionFactory::CreatePackageManagerSettings()
    {
        return winrt::create_instance<PackageManagerSettings>(CLSID_PackageManagerSettings, CLSCTX_ALL);
    }

    PackageManager WinGetProjectionFactory::TryCreatePackageManager()
    {
        if (useDev)
        {
            return winrt::try_create_instance<PackageManager>(CLSID_PackageManager2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<PackageManager>(CLSID_PackageManager, CLSCTX_ALL);
    }

    InstallOptions WinGetProjectionFactory::TryCreateInstallOptions()
    {
        if (useDev)
        {
            return winrt::try_create_instance<InstallOptions>(CLSID_InstallOptions2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<InstallOptions>(CLSID_InstallOptions, CLSCTX_ALL);
    }

    UninstallOptions WinGetProjectionFactory::TryCreateUninstallOptions()
    {
        if (useDev)
        {
            return winrt::try_create_instance<UninstallOptions>(CLSID_UninstallOptions2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<UninstallOptions>(CLSID_UninstallOptions, CLSCTX_ALL);
    }

    FindPackagesOptions WinGetProjectionFactory::TryCreateFindPackagesOptions()
    {
        if (useDev)
        {
            return winrt::try_create_instance<FindPackagesOptions>(CLSID_FindPackagesOptions2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<FindPackagesOptions>(CLSID_FindPackagesOptions, CLSCTX_ALL);
    }

    CreateCompositePackageCatalogOptions WinGetProjectionFactory::TryCreateCreateCompositePackageCatalogOptions()
    {
        if (useDev)
        {
            return winrt::try_create_instance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions, CLSCTX_ALL);
    }

    PackageMatchFilter WinGetProjectionFactory::TryCreatePackageMatchFilter()
    {
        if (useDev)
        {
            return winrt::try_create_instance<PackageMatchFilter>(CLSID_PackageMatchFilter2, CLSCTX_ALL);
        }
        return winrt::try_create_instance<PackageMatchFilter>(CLSID_PackageMatchFilter, CLSCTX_ALL);
    }

    PackageManagerSettings WinGetProjectionFactory::TryCreatePackageManagerSettings()
    {
        return winrt::try_create_instance<PackageManagerSettings>(CLSID_PackageManagerSettings, CLSCTX_ALL);
    }
}
