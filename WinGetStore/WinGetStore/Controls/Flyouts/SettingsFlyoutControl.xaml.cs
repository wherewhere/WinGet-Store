using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Globalization;
using Windows.Globalization;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinGetStore.Helpers;
using WinGetStore.ViewModels.SettingsPages;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace WinGetStore.Controls
{
    public sealed partial class SettingsFlyoutControl : SettingsFlyout
    {
        private Action<bool> UISettingChanged;

        internal SettingsViewModel Provider;

        public SettingsFlyoutControl()
        {
            InitializeComponent();
            ResourceDictionary ThemeResources = new() { Source = new Uri("ms-appx:///Styles/SettingsFlyout.xaml") };
            Style = (Style)ThemeResources["DefaultSettingsFlyoutStyle"];
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            UISettingChanged = (x) => RequestedTheme = x ? ElementTheme.Dark : ElementTheme.Light;
            if (Provider == null)
            {
                DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
                Provider = SettingsViewModel.Caches.TryGetValue(dispatcher, out SettingsViewModel provider) ? provider : new SettingsViewModel(dispatcher);
            }
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
            DataContext = Provider;
            _ = Provider.Refresh();
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            switch (ComboBox.Tag.ToString())
            {
                case "Language":
                    string lang = SettingsHelper.Get<string>(SettingsHelper.CurrentLanguage);
                    lang = lang == LanguageHelper.AutoLanguageCode ? LanguageHelper.GetCurrentLanguage() : lang;
                    CultureInfo culture = new(lang);
                    ComboBox.SelectedItem = culture;
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ComboBox = sender as ComboBox;
            switch (ComboBox.Tag.ToString())
            {
                case "Language":
                    CultureInfo culture = ComboBox.SelectedItem as CultureInfo;
                    if (culture.Name != LanguageHelper.GetCurrentLanguage())
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, culture.Name);
                    }
                    else
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
                        SettingsHelper.Set(SettingsHelper.CurrentLanguage, LanguageHelper.AutoLanguageCode);
                    }
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Rate":
                    _ = Launcher.LaunchUriAsync(new Uri("http://afdian.net/@wherewhere"));
                    break;
                case "Group":
                    _ = Launcher.LaunchUriAsync(new Uri("https://t.me/PavingBase"));
                    break;
                case "Reset":
                    Reset.Flyout?.Hide();
                    ApplicationData.Current.LocalSettings.Values.Clear();
                    SettingsHelper.SetDefaultSettings();
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
            switch ((sender as FrameworkElement).Tag.ToString())
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
            FrameworkElement element = sender as FrameworkElement;
            if (element?.FindDescendant("Title") is TextBlock title)
            {
                title.IsTextSelectionEnabled = true;
            }
            if (element?.FindDescendant("Message") is TextBlock message)
            {
                message.IsTextSelectionEnabled = true;
            }
        }

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri(e.Link));
    }
}
