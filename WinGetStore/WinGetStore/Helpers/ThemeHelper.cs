using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace WinGetStore.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        // Keep reference so it does not get optimized/garbage collected
        public static UISettings UISettings;

        static ThemeHelper()
        {
            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        public static void Initialize(Window window)
        {
            UpdateSystemCaptionButtonColors(window);
        }

        private static void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateSystemCaptionButtonColors();
        }

        public static bool IsDarkTheme() => UISettings.GetColorValue(UIColorType.Background) == Colors.Black;

        public static void UpdateSystemCaptionButtonColors()
        {
            bool IsDark = IsDarkTheme();
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            foreach (Window window in WindowHelper.ActiveWindows)
            {
                _ = window.Dispatcher.AwaitableRunAsync(() =>
                {
                    if (UIHelper.HasStatusBar)
                    {
                        StatusBar StatusBar = StatusBar.GetForCurrentView();
                        StatusBar.ForegroundColor = ForegroundColor;
                        StatusBar.BackgroundColor = BackgroundColor;
                        StatusBar.BackgroundOpacity = 0; // 透明度
                    }
                    else
                    {
                        bool ExtendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
                        ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                        TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                        TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                        TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
                    }
                });
            }
        }

        public static void UpdateSystemCaptionButtonColors(Window window)
        {
            _ = window.Dispatcher.AwaitableRunAsync(() =>
            {
                bool IsDark = IsDarkTheme();
                bool IsHighContrast = new AccessibilitySettings().HighContrast;

                Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
                Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

                if (UIHelper.HasStatusBar)
                {
                    StatusBar StatusBar = StatusBar.GetForCurrentView();
                    StatusBar.ForegroundColor = ForegroundColor;
                    StatusBar.BackgroundColor = BackgroundColor;
                    StatusBar.BackgroundOpacity = 0; // 透明度
                }
                else
                {
                    bool ExtendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
                    ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                    TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                    TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                    TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = ExtendViewIntoTitleBar ? Colors.Transparent : BackgroundColor;
                }
            });
        }
    }
}
