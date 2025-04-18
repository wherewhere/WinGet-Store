﻿using Microsoft.Management.Deployment;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Controls;
using WinGetStore.Controls.Dialogs;
using WinGetStore.ViewModels.ManagerPages;
#region using muxc = Microsoft.UI.Xaml.Controls;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;
#endregion

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.ManagerPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManagerPage : Page
    {
        private readonly ManagerViewModel Provider;

        public ManagerPage()
        {
            InitializeComponent();
            Provider = new ManagerViewModel(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Provider.MatchResults?.Count <= 0)
            {
                _ = Provider.Refresh();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case nameof(ActionButtonOne):
                    _ = Provider?.Refresh();
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case "Versions":
                    VersionsDialog dialog = new(element.Tag as CatalogPackage);
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
                    dataPackage.Properties.Title = shareString[15..];
                    dataPackage.Properties.Description = shareString;
                    Clipboard.SetContent(dataPackage);
                    break;
                default:
                    break;
            }
        }

        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            _ = Provider?.Refresh();
            if (e != null) { e.Handled = true; }
        }

        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args) => _ = Provider?.Refresh();
    }
}
