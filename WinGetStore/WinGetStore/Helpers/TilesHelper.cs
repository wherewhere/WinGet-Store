using AdaptiveCards;
using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using WinGetStore.Common;
using WinGetStore.ViewModels.ManagerPages;
using WinGetStore.WinRT;

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
                SettingsHelper.LogManager.GetLogger(nameof(TilesHelper)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static void UpdateTiles(this IEnumerable<XmlDocument> tileContents)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.EnableNotificationQueue(true);
                tileContents.ForEach(tileContent =>
                {
                    TileNotification tileNotification = new(tileContent);
                    tileUpdater.Update(tileNotification);
                });
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(TilesHelper)).Error(ex.ExceptionToMessage(), ex);
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

        public static async Task UpdateStartMenuCompanionAsync(this AdaptiveCard adaptiveCard, string fileName = "StartMenuCompanion.json")
        {
            try
            {
                StorageFile file = await StorageFileHelper.WriteTextToLocalFileAsync(adaptiveCard.ToJson(), fileName).ConfigureAwait(false);
                FileInfo info = new(file.Path);
                FileSecurity security = info.GetAccessControl();
                // Add Shell Experience Capability SID
                security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-3-1024-3167453650-624722384-889205278-321484983-714554697-3592933102-807660695-1632717421"), FileSystemRights.ReadAndExecute, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                info.SetAccessControl(security);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(TilesHelper)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static AdaptiveCard CreateStartMenuCompanion(IEnumerable<CatalogPackage> catalogPackages)
        {
            AdaptiveCard card = new(new AdaptiveSchemaVersion(1, 1))
            {
                Body =
                {
                    new AdaptiveColumnSet
                    {
                        Columns =
                        {
                            new AdaptiveColumn
                            {
                                Width = "8px"
                            },
                            new AdaptiveColumn
                            {
                                Items =
                                {
                                    new AdaptiveTextBlock(ResourceLoader.GetForViewIndependentUse().GetString("StartMenuTitle"))
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Spacing = AdaptiveSpacing.None
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = "8px"
                            }
                        },
                        Spacing = AdaptiveSpacing.None
                    }
                }
            };
            card.Body.AddRange(catalogPackages.Select(CreateCard));
            static AdaptiveContainer CreateCard(CatalogPackage catalogPackage)
            {
                return new AdaptiveContainer
                {
                    Items =
                    {
                        new AdaptiveColumnSet
                        {
                            Columns =
                            {
                                new AdaptiveColumn
                                {
                                    Width = "8px"
                                },
                                new AdaptiveColumn
                                {
                                    Items =
                                    {
                                        new AdaptiveTextBlock(catalogPackage.Name)
                                        {
                                            Weight = AdaptiveTextWeight.Bolder
                                        },
                                        new AdaptiveColumnSet
                                        {
                                            Columns =
                                            {
                                                new AdaptiveColumn
                                                {
                                                    Items =
                                                    {
                                                        new AdaptiveTextBlock(catalogPackage.Id)
                                                        {
                                                            IsSubtle = true,
                                                            Size = AdaptiveTextSize.Small,
                                                            Spacing = AdaptiveSpacing.None
                                                        }
                                                    }
                                                },
                                                new AdaptiveColumn
                                                {
                                                    Items =
                                                    {
                                                        new AdaptiveTextBlock(catalogPackage.IsUpdateAvailable ? "可更新" : " ")
                                                        {
                                                            Color = AdaptiveTextColor.Accent,
                                                            IsSubtle = true,
                                                            Size = AdaptiveTextSize.Small,
                                                            Spacing = AdaptiveSpacing.None
                                                        }
                                                    },
                                                    Width = "auto"
                                                }
                                            },
                                            Spacing = AdaptiveSpacing.Small
                                        }
                                    }
                                },
                                new AdaptiveColumn
                                {
                                    Width = "8px"
                                }
                            }
                        }
                    }
                };
            }
            return card;
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

                IEnumerable<CatalogPackage> catalogPackages =
                    packagesResult.Matches.AsReader()
                                          .Where((x) => x.CatalogPackage.DefaultInstallVersion != null)
                                          .OrderByDescending(item => item.CatalogPackage.IsUpdateAvailable)
                                          .Select((x) => x.CatalogPackage);

                Task task = CreateStartMenuCompanion(catalogPackages).UpdateStartMenuCompanionAsync();

                CatalogPackage[] available = catalogPackages.Where(x => x.IsUpdateAvailable).ToArray();

                SetBadgeNumber((uint)available.Length);
                available.Take(5)
                         .Select(CreateTile)
                         .UpdateTiles();

                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
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
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
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
                SettingsHelper.LogManager.GetLogger(nameof(ManagerViewModel)).Error(ex.ExceptionToMessage());
                return null;
            }
        }
    }
}
