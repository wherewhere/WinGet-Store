using CommunityToolkit.WinUI;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WinGetStore.Helpers;
using WinGetStore.ViewModels.SettingsPages;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace WinGetStore.Controls
{
    public sealed partial class SettingsFlyoutControl : SettingsFlyout
    {
        private readonly SettingsViewModel Provider;

        public SettingsFlyoutControl()
        {
            InitializeComponent();
            Provider = SettingsViewModel.Caches.TryGetValue(Dispatcher, out SettingsViewModel provider) ? provider : new SettingsViewModel(Dispatcher);
            ResourceDictionary ThemeResources = new() { Source = new Uri("ms-appx:///Styles/SettingsFlyout.xaml") };
            Style = (Style)ThemeResources["DefaultSettingsFlyoutStyle"];
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged += OnUISettingChanged;
            _ = Refresh();
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e) => ThemeHelper.UISettingChanged -= OnUISettingChanged;

        private void OnUISettingChanged(ApplicationTheme mode) => RequestedTheme = (ElementTheme)(mode + 1);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element?.Tag.ToString())
            {
                case "Rate":
                    _ = Launcher.LaunchUriAsync(new Uri("http://afdian.com/@wherewhere"));
                    break;
                case "Group":
                    _ = Launcher.LaunchUriAsync(new Uri("https://t.me/PavingBase"));
                    break;
                case "Reset":
                    SettingsHelper.LocalObject.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is FlyoutBase flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Refresh(true);
                    break;
                case "CheckUpdate":
                    Provider.CheckUpdate();
                    break;
                case "WinGetDownloadSettings":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp?productid=9NBLGGH4NNS1&mode=mini"));
                    break;
                default:
                    break;
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element?.Tag.ToString())
            {
                case "LogFolder":
                    _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "WindowsColor":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
                    break;
                default:
                    break;
            }
        }

        private void InfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not DependencyObject element) { return; }
            if (element.FindDescendant("Title") is TextBlock title)
            {
                title.IsTextSelectionEnabled = true;
            }
            if (element.FindDescendant("Message") is TextBlock message)
            {
                message.IsTextSelectionEnabled = true;
            }
        }

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));
    }
}
