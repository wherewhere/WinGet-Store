using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinGetStore.Helpers;
using WinGetStore.ViewModels;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace WinGetStore.Controls.Dialogs
{
    public sealed partial class VersionsDialog : ContentDialog
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("VersionsDialog");

        private readonly VersionsViewModel Provider;

        public VersionsDialog(VersionsViewModel provider)
        {
            InitializeComponent();
            Provider = provider;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Uri url = element.Tag?.ToString().ValidateAndGetUri();
            if (url != null)
            {
                _ = Launcher.LaunchUriAsync(url);
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement BackgroundElement = this.FindDescendant("BackgroundElement");
            if (BackgroundElement != null)
            {
                BackgroundElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            _ = Provider.Refresh();
        }

        internal static PackageAgreement[] PackageAgreementsToArray(IReadOnlyList<PackageAgreement> values) => values?.ToArray();

        internal static string PackageAgreementsToDescription(IReadOnlyList<PackageAgreement> values) => string.Format(_loader.GetString("InTotal"), values?.ToArray().Length);

        internal static Documentation[] DocumentationsToArray(IReadOnlyList<Documentation> values) => values?.ToArray();

        internal static string DocumentationsToDescription(IReadOnlyList<Documentation> values) => string.Format(_loader.GetString("InTotal"), values?.ToArray().Length);

        internal static Icon[] IconsToArray(IReadOnlyList<Icon> values) => values?.ToArray();

        internal static string IconsToDescription(IReadOnlyList<Icon> values) => string.Format(_loader.GetString("InTotal"), values?.ToArray().Length);

        internal static string[] TagsToArray(IReadOnlyList<string> values) => values?.ToArray();

        internal static string JoinTags(string separate, IReadOnlyList<string> values) => string.Join(separate, values?.ToArray());

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh(true);
    }
}
