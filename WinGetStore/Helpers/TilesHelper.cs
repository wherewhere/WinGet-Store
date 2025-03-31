using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using WinGetStore.Common;

namespace WinGetStore.Helpers
{
    public static class TilesHelper
    {
        public static void SetBadgeNumber(uint number)
        {
            BadgeNumericContent content = new(number);
            // Create the badge notification
            BadgeNotification badge = new(content.GetXml());
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static void UpdateTile(this XmlDocument tileContent)
        {
            try
            {
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.EnableNotificationQueue(true);
                TileNotification tileNotification = new(tileContent);
                tileUpdater.Update(tileNotification);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(TilesHelper)).LogError(ex, "Failed to update tile. {message} (0x{hResult:X})", ex.Message, ex.HResult);
            }
        }

        public static void UpdateTiles(this IEnumerable<XmlDocument> tileContents)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.EnableNotificationQueue(true);
                foreach (XmlDocument tileContent in tileContents)
                {
                    TileNotification tileNotification = new(tileContent);
                    tileUpdater.Update(tileNotification);
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(TilesHelper)).LogError(ex, "Failed to update tiles. {message} (0x{hResult:X})", ex.Message, ex.HResult);
            }
        }

        public static XmlDocument CreateTile(this CatalogPackage catalogPackage)
        {
            XmlDocument xmlDocument = new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = catalogPackage.Name,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText
                                {
                                    Text = catalogPackage.Id,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText
                                {
                                    Text = $"{catalogPackage.InstalledVersion?.Version ?? "null"} -> {catalogPackage.DefaultInstallVersion?.Version ?? "null"}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = catalogPackage.Name,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = catalogPackage.Id,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = $"{catalogPackage.InstalledVersion?.Version ?? "null"} -> {catalogPackage.DefaultInstallVersion?.Version ?? "null"}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    },

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = catalogPackage.Name,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = catalogPackage.Id,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = $"{catalogPackage.InstalledVersion?.Version ?? "null"} -> {catalogPackage.DefaultInstallVersion?.Version ?? "null"}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            }.GetXml();

            int i = 1;
            xmlDocument.GetElementsByTagName("binding")
                       .FirstOrDefault(x => x.Attributes?.GetNamedItem("template")?.InnerText == "TileWide").ChildNodes
                       .Where(x => x.NodeName == "text")
                       .OfType<XmlElement>()
                       .Take(3)
                       .ToArray()
                       .ForEach(x => x.SetAttribute("id", $"{i++}"));
            return xmlDocument;
        }

        public static async Task UpdateAvailablePackageAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                PackageCatalog packageCatalog = await CreatePackageCatalogAsync();
                if (packageCatalog is null) { return; }

                FindPackagesResult packagesResult = await TryFindPackageInCatalogAsync(packageCatalog);
                if (packagesResult is null) { return; }

                CatalogPackage[] available =
                    [.. packagesResult.Matches.AsReader()
                                              .Where(x => x.CatalogPackage is { DefaultInstallVersion: not null, IsUpdateAvailable: true })
                                              .Select(x => x.CatalogPackage)];

                SetBadgeNumber((uint)available.Length);
                available.Take(5)
                         .Select(CreateTile)
                         .UpdateTiles();
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(TilesHelper)).LogError(ex, "Failed to update available package tile. {message} (0x{hResult:X})", ex.Message, ex.HResult);
                return;
            }
        }

        private static async Task<PackageCatalog> CreatePackageCatalogAsync()
        {
            try
            {
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                if (packageManager is null) { return null; }

                IReadOnlyList<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs();
                if (packageCatalogReferences?.Count is not > 0) { return null; }

                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.AddRange(packageCatalogReferences.AsReader());
                createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;

                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                return connectResult.PackageCatalog;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(TilesHelper)).LogError(ex, "Failed to create package catalog. {message} (0x{hResult:X})", ex.Message, ex.HResult);
                return null;
            }
        }

        private static async Task<FindPackagesResult> TryFindPackageInCatalogAsync(PackageCatalog catalog)
        {
            try
            {
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
                return await catalog.FindPackagesAsync(findPackagesOptions);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(TilesHelper)).LogError(ex, "Failed to find packages. {message} (0x{hResult:X})", ex.Message, ex.HResult);
                return null;
            }
        }
    }
}
