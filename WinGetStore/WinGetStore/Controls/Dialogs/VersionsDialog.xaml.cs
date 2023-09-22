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
            DataContext = Provider;
            _ = Provider.Refresh();
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
        }

        internal static List<PackageAgreement> PackageAgreementsToList(IReadOnlyList<PackageAgreement> values) => values.ToList();

        internal static string PackageAgreementsToDescription(IReadOnlyList<PackageAgreement> values) => string.Format(_loader.GetString("InTotal"), values.ToList().Count);

        internal static List<Documentation> DocumentationsToList(IReadOnlyList<Documentation> values) => values.ToList();

        internal static string DocumentationsToDescription(IReadOnlyList<Documentation> values) => string.Format(_loader.GetString("InTotal"), values.ToList().Count);

        internal static List<Icon> IconsToList(IReadOnlyList<Icon> values) => values.ToList();

        internal static string IconsToDescription(IReadOnlyList<Icon> values) => string.Format(_loader.GetString("InTotal"), values.ToList().Count);

        internal static List<string> TagsToList(IReadOnlyList<string> values) => values.ToList();

        internal static string JoinTags(string separate, IReadOnlyList<string> values) => string.Join(separate, values.ToList());
    }
}
