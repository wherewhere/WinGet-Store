using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Controls;
using WinGetStore.Pages.ManagerPages;
using WinGetStore.Pages.SettingsPages;
using WinGetStore.ViewModels.ManagerPages;
#region using muxc = Microsoft.UI.Xaml.Controls;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode;
using NavigationViewDisplayModeChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using NavigationViewPaneClosingEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs;
using NavigationViewPaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode;
#endregion

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace WinGetStore.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly HashSet<(string Tag, Type Page)> _pages =
        [
            ("Home", typeof(ManagerPage)),
            ("Downloads", typeof(DownloadsPage)),
            ("Settings", typeof(SettingsPage))
        ];

        public MainPage()
        {
            InitializeComponent();
            SearchBoxHolder.RegisterPropertyChangedCallback(Slot.IsStretchProperty, new DependencyPropertyChangedCallback(OnIsStretchProperty));
            NavigationView.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, new DependencyPropertyChangedCallback(OnIsBackButtonVisibleChanged));
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush")) { BackdropMaterial.SetApplyToRootOrPageBackground(this, true); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current?.SetTitleBar(DragRegion);
            NavigationView_Navigate("Home", new EntranceNavigationTransitionInfo());
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            AppTitleText.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "WinGet Store";
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SetTitleBar(null);
            SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateAppTitle(sender);
        }

        public static string GetAppTitleFromSystem => Package.Current.DisplayName;

        private void OnIsStretchProperty(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is Slot)
            {
                UpdateAppTitleIcon();
            }
        }

        private void OnIsBackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void NavigationView_Navigate(string NavItemTag, NavigationTransitionInfo TransitionInfo, object vs = null)
        {
            Type _page = null;

            (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Tag.Equals(NavItemTag, StringComparison.Ordinal));
            _page = item.Page;
            // Get the page type before navigation so you can prevent duplicate
            // entries in the back stack.
            Type PreNavPageType = NavigationViewFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Equals(PreNavPageType, _page))
            {
                _ = NavigationViewFrame.Navigate(_page, vs, TransitionInfo);
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => _ = TryGoBack();

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                string NavItemTag = args.InvokedItemContainer.Tag.ToString();
                NavigationView_Navigate(NavItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private bool TryGoBack()
        {
            if (!Dispatcher.HasThreadAccess)
            { return false; }

            if (!NavigationViewFrame.CanGoBack)
            { return false; }

            // Don't go back if the nav pane is overlayed.
            if (NavigationView.IsPaneOpen &&
                (NavigationView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal))
            { return false; }

            NavigationViewFrame.GoBack();
            return true;
        }

        private void On_Navigated(object _, NavigationEventArgs e)
        {
            NavigationView.IsBackEnabled = NavigationViewFrame.CanGoBack;
            NavigationView.IsBackButtonVisible = NavigationViewFrame.CanGoBack
                ? NavigationViewBackButtonVisible.Visible
                : NavigationViewBackButtonVisible.Collapsed;
            if (NavigationViewFrame.SourcePageType != null)
            {
                (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                if (item.Tag != null)
                {
                    NavigationViewItem SelectedItem = NavigationView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(n => n.Tag.Equals(item.Tag))
                            ?? NavigationView.FooterMenuItems
                                .OfType<NavigationViewItem>()
                                .FirstOrDefault(n => n.Tag.Equals(item.Tag));
                    NavigationView.SelectedItem = SelectedItem;
                }
            }
        }

        private void NavigationViewControl_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateLeftPaddingColumn();
        }

        private void NavigationViewControl_PaneOpening(NavigationView sender, object args)
        {
            UpdateLeftPaddingColumn();
        }

        private void UpdateLeftPaddingColumn()
        {
            LeftPaddingColumn.Width = NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top
                ? NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                    ? new GridLength(48) : new GridLength(0)
                    : NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal
                        ? NavigationView.IsPaneOpen ? new GridLength(72)
                        : NavigationView.IsPaneToggleButtonVisible
                            ? NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                            ? new GridLength(88) : new GridLength(48)
                                : NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                                ? new GridLength(48) : new GridLength(0)
                                    : NavigationView.IsBackButtonVisible != NavigationViewBackButtonVisible.Collapsed
                                    ? new GridLength(48) : new GridLength(0);
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void UpdateAppTitleIcon()
        {
            AppTitleIcon.Margin = SearchBoxHolder.IsStretch
                && NavigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top
                && NavigationView.DisplayMode != NavigationViewDisplayMode.Minimal
                    ? NavigationView.IsBackButtonVisible == NavigationViewBackButtonVisible.Visible
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(24.5, 0, 24, 0)
                    : NavigationView.IsBackButtonVisible == NavigationViewBackButtonVisible.Visible
                        || (NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal
                            && NavigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top
                            && NavigationView.IsPaneToggleButtonVisible)
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(16, 0, 16, 0);
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            //ensure the custom title bar does not overlap window caption controls
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text, Dispatcher));
            }
        }
    }
}
