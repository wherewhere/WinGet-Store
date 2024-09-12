using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinGetStore.Controls;
using WinGetStore.Helpers;
using WinGetStore.Pages;
using WinGetStore.Pages.ManagerPages;
using WinGetStore.ViewModels.ManagerPages;

namespace WinGetStore.Common
{
    public static class SettingsPaneRegister
    {
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane") && CheckSearchExtension();
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public static void Register(Window window)
        {
            try
            {
                if (IsSearchPaneSupported)
                {
                    SearchPane searchPane = SearchPane.GetForCurrentView();
                    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                    searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                }

                if (IsSettingsPaneSupported)
                {
                    SettingsPane searchPane = SettingsPane.GetForCurrentView();
                    searchPane.CommandsRequested -= OnCommandsRequested;
                    searchPane.CommandsRequested += OnCommandsRequested;
                    window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                    window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsPaneRegister)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static void Unregister(Window window)
        {
            try
            {
                if (IsSearchPaneSupported)
                {
                    SearchPane searchPane = SearchPane.GetForCurrentView();
                    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                }

                if (IsSettingsPaneSupported)
                {
                    SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                    window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsPaneRegister)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        private static void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            if (args.QueryText is string keyWord && !string.IsNullOrEmpty(keyWord))
            {
                MainPage page = Window.Current?.Content?.FindDescendant<MainPage>();
                _ = page.NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(keyWord));
            }
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
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

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.HasFlag(CoreAcceleratorKeyEventType.KeyDown) || args.EventType.HasFlag(CoreAcceleratorKeyEventType.SystemKeyUp))
            {
                CoreWindow window = CoreWindow.GetForCurrentThread();
                CoreVirtualKeyStates ctrl = window.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = window.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X when IsSettingsPaneSupported:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q when IsSearchPaneSupported:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }

        private static bool CheckSearchExtension()
        {
            try
            {
                XDocument doc = XDocument.Load(Path.Combine(Package.Current.InstalledLocation.Path, "AppxManifest.xml"));
                XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
                IEnumerable<XElement> extensions = doc.Root.Descendants(ns + "Extension");
                if (extensions != null)
                {
                    foreach (XElement extension in extensions)
                    {
                        XAttribute category = extension.Attribute("Category");
                        if (category != null && category.Value == "windows.search")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(SettingsPaneRegister)).Error(ex.ExceptionToMessage(), ex);
            }
            return false;
        }
    }
}
