using Microsoft.Management.Deployment;
using System;
using System.Runtime.InteropServices;
using WinRT;

namespace WinGetStore.Common
{
    public static partial class WinGetProjectionFactory
    {
        private const uint CLSCTX_ALL = 1 | 2 | 4 | 16;
        public static readonly Guid CLSID_IUnknown = new("00000000-0000-0000-C000-000000000046");

        // CLSIDs for WinGet package
        private static readonly Guid CLSID_PackageManager = new(0xC53A4F16, 0x787E, 0x42A4, 0xB3, 0x04, 0x29, 0xEF, 0xFB, 0x4B, 0xF5, 0x97);                        //C53A4F16-787E-42A4-B304-29EFFB4BF597
        private static readonly Guid CLSID_FindPackagesOptions = new(0x572DED96, 0x9C60, 0x4526, 0x8F, 0x92, 0xEE, 0x7D, 0x91, 0xD3, 0x8C, 0x1A);                   //572DED96-9C60-4526-8F92-EE7D91D38C1A
        private static readonly Guid CLSID_CreateCompositePackageCatalogOptions = new(0x526534B8, 0x7E46, 0x47C8, 0x84, 0x16, 0xB1, 0x68, 0x5C, 0x32, 0x7D, 0x37);  //526534B8-7E46-47C8-8416-B1685C327D37
        private static readonly Guid CLSID_InstallOptions = new(0x1095f097, 0xEB96, 0x453B, 0xB4, 0xE6, 0x16, 0x13, 0x63, 0x7F, 0x3B, 0x14);                        //1095F097-EB96-453B-B4E6-1613637F3B14
        private static readonly Guid CLSID_UninstallOptions = new(0xE1D9A11E, 0x9F85, 0x4D87, 0x9C, 0x17, 0x2B, 0x93, 0x14, 0x3A, 0xDB, 0x8D);                      //E1D9A11E-9F85-4D87-9C17-2B93143ADB8D
        private static readonly Guid CLSID_PackageMatchFilter = new(0xD02C9DAF, 0x99DC, 0x429C, 0xB5, 0x03, 0x4E, 0x50, 0x4E, 0x4A, 0xB0, 0x00);                    //D02C9DAF-99DC-429C-B503-4E504E4AB000
        private static readonly Guid CLSID_ConfigurationStaticFunctions = new(0x73D763B7, 0x2937, 0x432F, 0xA9, 0x7A, 0xD9, 0x8A, 0x4A, 0x59, 0x61, 0x26);          //73D763B7-2937-432F-A97A-D98A4A596126
        private static readonly Guid CLSID_DownloadOptions = new(0x4288DF96, 0xFDC9, 0x4B68, 0xB4, 0x03, 0x19, 0x3D, 0xBB, 0xF5, 0x6A, 0x24);                       //4288DF96-FDC9-4B68-B403-193DBBF56A24

        private static readonly Guid CLSID_PackageManagerSettings = new(0x80CF9D63, 0x5505, 0x4342, 0xB9, 0xB4, 0xBB, 0x87, 0x89, 0x5C, 0xA8, 0xBB);                //80CF9D63-5505-4342-B9B4-BB87895CA8BB

        // CLSIDs for WinGetDev package
        private static readonly Guid CLSID_PackageManager_Dev = new(0x74CB3139, 0xB7C5, 0x4B9E, 0x93, 0x88, 0xE6, 0x61, 0x6D, 0xEA, 0x28, 0x8C);                        //74CB3139-B7C5-4B9E-9388-E6616DEA288C
        private static readonly Guid CLSID_FindPackagesOptions_Dev = new(0x1BD8FF3A, 0xEC50, 0x4F69, 0xAE, 0xEE, 0xDF, 0x4C, 0x9D, 0x3B, 0xAA, 0x96);                   //1BD8FF3A-EC50-4F69-AEEE-DF4C9D3BAA96
        private static readonly Guid CLSID_CreateCompositePackageCatalogOptions_Dev = new(0xEE160901, 0xB317, 0x4EA7, 0x9C, 0xC6, 0x53, 0x55, 0xC6, 0xD7, 0xD8, 0xA7);  //EE160901-B317-4EA7-9CC6-5355C6D7D8A7
        private static readonly Guid CLSID_InstallOptions_Dev = new(0x44FE0580, 0x62F7, 0x44D4, 0x9E, 0x91, 0xAA, 0x96, 0x14, 0xAB, 0x3E, 0x86);                        //44FE0580-62F7-44D4-9E91-AA9614AB3E86
        private static readonly Guid CLSID_UninstallOptions_Dev = new(0xAA2A5C04, 0x1AD9, 0x46C4, 0xB7, 0x4F, 0x6B, 0x33, 0x4A, 0xD7, 0xEB, 0x8C);                      //AA2A5C04-1AD9-46C4-B74F-6B334AD7EB8C
        private static readonly Guid CLSID_PackageMatchFilter_Dev = new(0x3F85B9F4, 0x487A, 0x4C48, 0x90, 0x35, 0x29, 0x03, 0xF8, 0xA6, 0xD9, 0xE8);                    //3F85B9F4-487A-4C48-9035-2903F8A6D9E8
        private static readonly Guid CLSID_ConfigurationStaticFunctions_Dev = new(0xC9ED7917, 0x66AB, 0x4E31, 0xA9, 0x2A, 0xF6, 0x5F, 0x18, 0xEF, 0x79, 0x33);          //C9ED7917-66AB-4E31-A92A-F65F18EF7933
        private static readonly Guid CLSID_DownloadOptions_Dev = new(0x8EF324ED, 0x367C, 0x4880, 0x83, 0xE5, 0xBB, 0x2A, 0xBD, 0x0B, 0x72, 0xF6);                       //8EF324ED-367C-4880-83E5-BB2ABD0B72F6

        public static bool IsUseDev { get; set; }

        public static bool IsWinGetInstalled
        {
            get
            {
                int hresult = CoCreateInstance(CLSID_PackageManager, 0, CLSCTX_ALL, CLSID_IUnknown, out nint result);
                return hresult == 0 && result != 0;
            }
        }

        public static bool IsWinGetDevInstalled
        {
            get
            {
                int hresult = CoCreateInstance(CLSID_PackageManager_Dev, 0, CLSCTX_ALL, CLSID_IUnknown, out nint result);
                return hresult == 0 && result != 0;
            }
        }

        public static PackageManager CreatePackageManager()
        {
            return IsUseDev
                ? CreateInstance<PackageManager>(CLSID_PackageManager_Dev, CLSCTX_ALL)
                : CreateInstance<PackageManager>(CLSID_PackageManager, CLSCTX_ALL);
        }

        public static FindPackagesOptions CreateFindPackagesOptions()
        {
            return IsUseDev
                ? CreateInstance<FindPackagesOptions>(CLSID_FindPackagesOptions_Dev, CLSCTX_ALL)
                : CreateInstance<FindPackagesOptions>(CLSID_FindPackagesOptions, CLSCTX_ALL);
        }

        public static CreateCompositePackageCatalogOptions CreateCreateCompositePackageCatalogOptions()
        {
            return IsUseDev
                ? CreateInstance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions_Dev, CLSCTX_ALL)
                : CreateInstance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions, CLSCTX_ALL);
        }

        public static InstallOptions CreateInstallOptions()
        {
            return IsUseDev
                ? CreateInstance<InstallOptions>(CLSID_InstallOptions_Dev, CLSCTX_ALL)
                : CreateInstance<InstallOptions>(CLSID_InstallOptions, CLSCTX_ALL);
        }

        public static UninstallOptions CreateUninstallOptions()
        {
            return IsUseDev
                ? CreateInstance<UninstallOptions>(CLSID_UninstallOptions_Dev, CLSCTX_ALL)
                : CreateInstance<UninstallOptions>(CLSID_UninstallOptions, CLSCTX_ALL);
        }

        public static PackageMatchFilter CreatePackageMatchFilter()
        {
            return IsUseDev
                ? CreateInstance<PackageMatchFilter>(CLSID_PackageMatchFilter_Dev, CLSCTX_ALL)
                : CreateInstance<PackageMatchFilter>(CLSID_PackageMatchFilter, CLSCTX_ALL);
        }

        public static DownloadOptions CreateDownloadOptions()
        {
            return IsUseDev
                ? CreateInstance<DownloadOptions>(CLSID_DownloadOptions_Dev, CLSCTX_ALL)
                : CreateInstance<DownloadOptions>(CLSID_DownloadOptions, CLSCTX_ALL);
        }

        public static PackageManagerSettings CreatePackageManagerSettings()
        {
            return CreateInstance<PackageManagerSettings>(CLSID_PackageManagerSettings, CLSCTX_ALL);
        }

        public static PackageManager TryCreatePackageManager()
        {
            return IsUseDev
                ? TryCreateInstance<PackageManager>(CLSID_PackageManager_Dev, CLSCTX_ALL)
                : TryCreateInstance<PackageManager>(CLSID_PackageManager, CLSCTX_ALL);
        }

        public static FindPackagesOptions TryCreateFindPackagesOptions()
        {
            return IsUseDev
                ? TryCreateInstance<FindPackagesOptions>(CLSID_FindPackagesOptions_Dev, CLSCTX_ALL)
                : TryCreateInstance<FindPackagesOptions>(CLSID_FindPackagesOptions, CLSCTX_ALL);
        }

        public static CreateCompositePackageCatalogOptions TryCreateCreateCompositePackageCatalogOptions()
        {
            return IsUseDev
                ? TryCreateInstance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions_Dev, CLSCTX_ALL)
                : TryCreateInstance<CreateCompositePackageCatalogOptions>(CLSID_CreateCompositePackageCatalogOptions, CLSCTX_ALL);
        }

        public static InstallOptions TryCreateInstallOptions()
        {
            return IsUseDev
                ? TryCreateInstance<InstallOptions>(CLSID_InstallOptions_Dev, CLSCTX_ALL)
                : TryCreateInstance<InstallOptions>(CLSID_InstallOptions, CLSCTX_ALL);
        }

        public static UninstallOptions TryCreateUninstallOptions()
        {
            return IsUseDev
                ? TryCreateInstance<UninstallOptions>(CLSID_UninstallOptions_Dev, CLSCTX_ALL)
                : TryCreateInstance<UninstallOptions>(CLSID_UninstallOptions, CLSCTX_ALL);
        }

        public static PackageMatchFilter TryCreatePackageMatchFilter()
        {
            return IsUseDev
                ? TryCreateInstance<PackageMatchFilter>(CLSID_PackageMatchFilter_Dev, CLSCTX_ALL)
                : TryCreateInstance<PackageMatchFilter>(CLSID_PackageMatchFilter, CLSCTX_ALL);
        }

        public static DownloadOptions TryCreateDownloadOptions()
        {
            return IsUseDev
                ? TryCreateInstance<DownloadOptions>(CLSID_DownloadOptions_Dev, CLSCTX_ALL)
                : TryCreateInstance<DownloadOptions>(CLSID_DownloadOptions, CLSCTX_ALL);
        }

        public static PackageManagerSettings TryCreatePackageManagerSettings()
        {
            return TryCreateInstance<PackageManagerSettings>(CLSID_PackageManagerSettings, CLSCTX_ALL);
        }

        internal static T CreateInstance<T>(Guid rclsid, uint dwClsContext = CLSCTX_ALL)
        {
            int hresult = CoCreateInstance(rclsid, 0, dwClsContext, CLSID_IUnknown, out nint result);
            if (hresult < 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
            return Marshaler<T>.FromAbi(result);
        }

        public static T TryCreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1) where T : class
        {
            int hresult = CoCreateInstance(rclsid, 0, dwClsContext, CLSID_IUnknown, out nint result);
            return hresult < 0 ? null : Marshaler<T>.FromAbi(result);
        }

        [LibraryImport("api-ms-win-core-com-l1-1-0.dll")]
        private static partial int CoCreateInstance(in Guid rclsid, nint pUnkOuter, uint dwClsContext, in Guid riid, out nint ppv);
    }
}
