using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinGetStore.ViewModels.ManagerPages;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.ManagerPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadsPage : Page
    {
        private readonly DownloadsViewModel Provider = new();

        public DownloadsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = Provider.Refresh();
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

        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            _ = Provider?.Refresh();
            if (e != null) { e.Handled = true; }
        }

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh();
    }
}
