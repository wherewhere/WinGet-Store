using System;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinGetStore.Controls;
using WinGetStore.Helpers;

namespace WinGetStore.Common
{
    public class SettingsPaneRegister
    {
        private UIElement element;

        public SettingsPaneRegister(Window window)
        {
            element = window.Content;

            if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
        }

        public SettingsPaneRegister(UIElement element)
        {
            this.element = element;

            if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
            }
        }

        public static SettingsPaneRegister Register(Window window) => new SettingsPaneRegister(window);

        public static SettingsPaneRegister Register(UIElement element) => new SettingsPaneRegister(element);

        public void Unregister()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                element.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }

            element = null;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("SettingsPane");
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Settings",
                    loader.GetString("Settings"),
                    async (handler) => new SettingsFlyoutControl { RequestedTheme = await ThemeHelper.GetActualThemeAsync() }.Show()));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedback",
                    loader.GetString("Feedback"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite/issues"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "LogFolder",
                    loader.GetString("LogFolder"),
                    async (handler) => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Translate",
                    loader.GetString("Translate"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://crowdin.com/project/CoolapkLite"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Repository",
                    loader.GetString("Repository"),
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite"))));
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }
    }
}
