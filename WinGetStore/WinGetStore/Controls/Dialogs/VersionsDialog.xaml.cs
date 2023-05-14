using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static List<Documentation> DocumentationsToList(IReadOnlyList<Documentation> values) => values.ToList();

        internal static List<string> TagsToList(IReadOnlyList<string> values) => values.ToList();

        internal static string JoinTags(string separate, IReadOnlyList<string> values) => string.Join(separate, values.ToList());
    }
}
