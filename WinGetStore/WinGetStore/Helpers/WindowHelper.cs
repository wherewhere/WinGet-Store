using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using WinGetStore.Common;

namespace WinGetStore.Helpers
{
    /// <summary>
    /// Helpers class to allow the app to find the Window that contains an
    /// arbitrary <see cref="UIElement"/> (GetWindowForElement(UIElement)).
    /// To do this, we keep track of all active Windows. The app code must call
    /// <see cref="CreateWindowAsync(Action{Window})"/> rather than "new <see cref="Window"/>()"
    /// so we can keep track of all the relevant windows.
    /// </summary>
    public static class WindowHelper
    {
        public static bool IsAppWindowSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");
        public static bool IsXamlRootSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot");

        public static async Task<bool> CreateWindowAsync(Action<Window> launched)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = await newView.Dispatcher.AwaitableRunAsync(() =>
            {
                Window newWindow = Window.Current;
                launched(newWindow);
                newWindow.TrackWindow();
                Window.Current.Activate();
                return ApplicationView.GetForCurrentView().Id;
            });
            return await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        public static void TrackWindow(this Window window)
        {
            if (!ActiveWindows.ContainsKey(window.Dispatcher))
            {
                SettingsPaneRegister register = SettingsPaneRegister.Register(window);
                window.Closed += (sender, args) =>
                {
                    ActiveWindows.Remove(window.Dispatcher);
                    register.Unregister();
                    window = null;
                };
                ActiveWindows[window.Dispatcher] = window;
            }
        }

        public static Size GetXAMLRootSize(this UIElement element) =>
            IsXamlRootSupported && element.XamlRoot != null
                ? element.XamlRoot.Size
                : Window.Current is Window window
                    ? window.Bounds.ToSize()
                    : CoreApplication.MainView.CoreWindow.Bounds.ToSize();

        public static UIElement GetXAMLRoot(this UIElement element) =>
            IsXamlRootSupported && element.XamlRoot != null
                ? element.XamlRoot.Content
                : Window.Current is Window window
                    ? window.Content : null;

        public static void SetXAMLRoot(this UIElement element, UIElement target)
        {
            if (IsXamlRootSupported)
            {
                element.XamlRoot = target?.XamlRoot;
            }
        }

        public static Dictionary<CoreDispatcher, Window> ActiveWindows { get; } = [];
    }
}
