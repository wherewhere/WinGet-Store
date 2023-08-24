using Microsoft.Toolkit.Uwp.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using WinGetStore.Common;

namespace WinGetStore.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        // Keep reference so it does not get optimized/garbage collected
        public static UISettings UISettings;

        public static WeakEvent<bool> UISettingChanged { get; } = new WeakEvent<bool>();

        #region ActualTheme

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme => GetActualTheme();

        public static ElementTheme GetActualTheme() =>
            CurrentApplicationWindow == null
                ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                    ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                        && rootElement.RequestedTheme != ElementTheme.Default
                            ? rootElement.RequestedTheme
                            : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                    : UIHelper.AwaitByTaskCompleteSource(() =>
                        CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                            CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                && _rootElement.RequestedTheme != ElementTheme.Default
                                    ? _rootElement.RequestedTheme
                                    : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                            CoreDispatcherPriority.High));

        public static async Task<ElementTheme> GetActualThemeAsync() =>
            CurrentApplicationWindow == null
                ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                    ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                        && rootElement.RequestedTheme != ElementTheme.Default
                            ? rootElement.RequestedTheme
                            : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                    : await CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                        CurrentApplicationWindow.Content is FrameworkElement _rootElement
                            && _rootElement.RequestedTheme != ElementTheme.Default
                                ? _rootElement.RequestedTheme
                                : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                            CoreDispatcherPriority.High);

        #endregion

        #region RootTheme

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get => GetRootTheme();
            set => SetRootTheme(value);
        }

        public static ElementTheme GetRootTheme() =>
            CurrentApplicationWindow == null
                ? ElementTheme.Default
                : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                    ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                        ? rootElement.RequestedTheme
                        : ElementTheme.Default
                    : UIHelper.AwaitByTaskCompleteSource(() =>
                        CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                            CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                ? _rootElement.RequestedTheme
                                : ElementTheme.Default,
                            CoreDispatcherPriority.High));

        public static async Task<ElementTheme> GetRootThemeAsync() =>
            CurrentApplicationWindow == null
                ? ElementTheme.Default
                : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                    ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                        ? rootElement.RequestedTheme
                        : ElementTheme.Default
                    : await CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                        CurrentApplicationWindow.Content is FrameworkElement _rootElement
                            ? _rootElement.RequestedTheme
                            : ElementTheme.Default,
                        CoreDispatcherPriority.High);

        public static async void SetRootTheme(ElementTheme value)
        {
            WindowHelper.ActiveWindows.ForEach(async (window) =>
            {
                await window.Dispatcher.ResumeForegroundAsync();
                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            });

            SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync());
        }

        public static async Task SetRootThemeAsync(ElementTheme value)
        {
            await Task.WhenAll(WindowHelper.ActiveWindows.Select(async (window) =>
            {
                await window.Dispatcher.ResumeForegroundAsync();
                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }));

            SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync());
        }

        #endregion

        static ThemeHelper()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = Window.Current;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        public static async void Initialize(Window window)
        {
            CurrentApplicationWindow ??= window;
            if (window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = await GetActualThemeAsync();
            }
            UpdateSystemCaptionButtonColors(window);
        }

        private static async void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateSystemCaptionButtonColors();
            UISettingChanged.Invoke(await IsDarkThemeAsync());
        }

        public static bool IsDarkTheme()
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static async Task<bool> IsDarkThemeAsync()
        {
            ElementTheme ActualTheme = await GetActualThemeAsync();
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static bool IsDarkTheme(this ElementTheme ActualTheme)
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static void UpdateSystemCaptionButtonColors(this ElementTheme theme)
        {
            bool isDark = theme.IsDarkTheme();
            UpdateSystemCaptionButtonColors(isDark);
        }

        public static async void UpdateSystemCaptionButtonColors()
        {
            await CurrentApplicationWindow.Dispatcher.ResumeForegroundAsync();
            bool isDark = await IsDarkThemeAsync();
            UpdateSystemCaptionButtonColors(isDark);
        }

        public static void UpdateSystemCaptionButtonColors(bool isDark) =>
            WindowHelper.ActiveWindows.ForEach((window) => window.UpdateSystemCaptionButtonColors(isDark));

        public static async void UpdateSystemCaptionButtonColors(this Window window)
        {
            await CurrentApplicationWindow.Dispatcher.ResumeForegroundAsync();
            bool isDark = await IsDarkThemeAsync();
            window.UpdateSystemCaptionButtonColors(isDark);
        }

        public static async void UpdateSystemCaptionButtonColors(this Window window, bool isDark)
        {
            bool IsDark = isDark;
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            await window.Dispatcher.ResumeForegroundAsync();

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
        }
    }
}
