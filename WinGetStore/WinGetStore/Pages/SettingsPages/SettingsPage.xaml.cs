using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinGetStore.ViewModels.SettingsPages;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel Provider = new();

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (DataContext is not SettingsViewModel)
            {
                DataContext = Provider;
            }
            _ = Provider.Refresh();
        }

    }
}
