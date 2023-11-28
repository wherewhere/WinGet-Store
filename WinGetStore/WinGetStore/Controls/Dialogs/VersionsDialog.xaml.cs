using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
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
            if (sender is not FrameworkElement element) { return; }
            if (element.Tag?.ToString().TryGetUri(out Uri url) == true)
            {
                _ = Launcher.LaunchUriAsync(url);
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FindDescendant("BackgroundElement") is FrameworkElement BackgroundElement)
            {
                BackgroundElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }

        private static IEnumerable<T> GetEnumerable<T>(IReadOnlyList<T> values)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    yield return values[i];
                }
            }
        }

        internal static IEnumerable<PackageAgreement> PackageAgreementsToArray(IReadOnlyList<PackageAgreement> values) => GetEnumerable(values);

        internal static string PackageAgreementsToDescription(IReadOnlyList<PackageAgreement> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static IEnumerable<Documentation> DocumentationsToArray(IReadOnlyList<Documentation> values) => GetEnumerable(values);

        internal static string DocumentationsToDescription(IReadOnlyList<Documentation> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static IEnumerable<Icon> IconsToArray(IReadOnlyList<Icon> values) => GetEnumerable(values);

        internal static string IconsToDescription(IReadOnlyList<Icon> values) => string.Format(_loader.GetString("InTotal"), values.Count);

        internal static IEnumerable<string> TagsToArray(IReadOnlyList<string> values) => GetEnumerable(values);

        internal static string JoinTags(string separate, IReadOnlyList<string> values) => string.Join(separate, GetEnumerable(values));

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh(true);
    }
}
