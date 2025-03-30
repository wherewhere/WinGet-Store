using CommunityToolkit.WinUI;
using Microsoft.Management.Deployment;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinGetStore.ViewModels;
#region using muxc = Microsoft.UI.Xaml.Controls;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;
#endregion

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace WinGetStore.Controls.Dialogs
{
    public sealed partial class VersionsDialog : ContentDialog
    {
        private readonly VersionsViewModel Provider;

        public VersionsDialog(CatalogPackage package)
        {
            InitializeComponent();
            Provider = new VersionsViewModel(package, Dispatcher);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FindDescendant("BackgroundElement") is FrameworkElement BackgroundElement)
            {
                BackgroundElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args) => _ = Provider?.Refresh(true);
    }
}
