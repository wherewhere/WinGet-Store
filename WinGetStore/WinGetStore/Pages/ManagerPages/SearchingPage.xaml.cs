using Microsoft.Management.Deployment;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Controls;
using WinGetStore.Controls.Dialogs;
using WinGetStore.ViewModels.ManagerPages;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.ManagerPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchingPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(SearchingViewModel),
                typeof(SearchingPage),
                null);

        /// <summary>
        /// Get the <see cref="SearchingViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public SearchingViewModel Provider
        {
            get => (SearchingViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public SearchingPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel && Provider != ViewModel)
            {
                Provider = ViewModel;
                _ = Provider.Refresh();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case nameof(Filters):
                    FiltersDialog dialog = new(new(Provider.Selectors, Provider.Filters));
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        Provider.Selectors = dialog.Selectors;
                        Provider.Filters = dialog.Filters;
                        _ = Provider.Refresh();
                    }
                    break;
                case nameof(ActionButtonOne):
                    _ = Provider?.Refresh();
                    break;
                default:
                    break;
            }
        }

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

        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            _ = Provider?.Refresh();
            if (e != null) { e.Handled = true; }
        }

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh();
    }
}
