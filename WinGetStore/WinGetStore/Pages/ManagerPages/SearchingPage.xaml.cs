using Microsoft.Management.Deployment;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
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
        private SearchingViewModel Provider;

        public SearchingPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
                DataContext = Provider;
                _ = Provider.Refresh();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Filters":
                    FiltersDialog dialog = new(new(Provider.PackageMatchFilters));
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        Provider.PackageMatchFilters = dialog.PackageMatchFilters;
                        _ = Provider.Refresh();
                    }
                    break;
                case "ActionButtonOne":
                    _ = Provider?.Refresh();
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Versions":
                    VersionsDialog dialog = new(new(element.Tag as CatalogPackage));
                    _ = dialog.ShowAsync();
                    break;
            }
        }

        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => _ = Provider?.Refresh();

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh();
    }
}
