using Microsoft.Toolkit.Uwp.UI;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FindDescendant("BackgroundElement") is FrameworkElement BackgroundElement)
            {
                BackgroundElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh(true);
    }
}
