using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using WinGetStore.Common;
using WinGetStore.Controls.Dialogs;
using WinGetStore.Helpers;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace WinGetStore.Controls.DataTemplates
{
    public partial class PackageDataTemplates : ResourceDictionary
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("VersionsDialog");

        public PackageDataTemplates() => InitializeComponent();

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case "Versions":
                    VersionsDialog dialog = new(new(element.Tag as CatalogPackage));
                    _ = dialog.ShowAsync();
                    break;
                case "Upgrade":
                    (element.Tag as PackageControl).CheckToUpgrade();
                    break;
                case "Install":
                    (element.Tag as PackageControl).CheckToInstall();
                    break;
                case "Uninstall":
                    (element.Tag as PackageControl).CheckToUninstall();
                    break;
                case "Cancel":
                    (element.Tag as PackageControl).Progress?.Cancel();
                    break;
                case "Share":
                    DataPackage dataPackage = new();
                    string shareString = element.Tag?.ToString();
                    dataPackage.SetText(shareString);
                    dataPackage.Properties.Title = shareString.Substring(15);
                    dataPackage.Properties.Description = shareString;
                    Clipboard.SetContent(dataPackage);
                    break;
                default:
                    break;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            if (element.Tag?.ToString().TryGetUri(out Uri url) == true)
            {
                _ = Launcher.LaunchUriAsync(url);
            }
        }

        internal static VectorViewReader<PackageAgreement> GetPackageAgreementsReader(IReadOnlyList<PackageAgreement> values) => values.AsReader();

        internal static string PackageAgreementsToDescription(IReadOnlyList<PackageAgreement> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static VectorViewReader<Documentation> GetDocumentationsReader(IReadOnlyList<Documentation> values) => values.AsReader();

        internal static string DocumentationsToDescription(IReadOnlyList<Documentation> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static VectorViewReader<Icon> GetIconsReader(IReadOnlyList<Icon> values) => values.AsReader();

        internal static string IconsToDescription(IReadOnlyList<Icon> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static VectorViewReader<string> GetTagsReader(IReadOnlyList<string> values) => values.AsReader();

        internal static string JoinTags(string separate, IReadOnlyList<string> values) => string.Join(separate, values.AsReader());
    }
}
