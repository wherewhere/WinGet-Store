using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinGetStore.ViewModels.ManagerPages;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.ManagerPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InstallingPage : Page
    {
        private readonly InstallingViewModel Provider = new();

        public InstallingPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (DataContext is not InstallingViewModel)
            {
                DataContext = Provider;
            }
            _ = Provider.Refresh();
        }

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args) => _ = Provider?.Refresh();
    }
}
