using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Helpers;
using WinGetStore.ViewModels.SettingsPages;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace WinGetStore.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(SettingsViewModel),
                typeof(SettingsPage),
                null);

        /// <summary>
        /// Get the <see cref="SettingsViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public SettingsViewModel Provider
        {
            get => (SettingsViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public SettingsPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Provider == null)
            {
                DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
                Provider = SettingsViewModel.Caches.TryGetValue(dispatcher, out SettingsViewModel provider) ? provider : new SettingsViewModel(dispatcher);
            }
            _ = Refresh();
        }

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

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
