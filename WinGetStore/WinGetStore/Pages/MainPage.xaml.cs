using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Controls;
using WinGetStore.Pages.ManagerPages;
using WinGetStore.Pages.SettingsPages;
using WinGetStore.ViewModels.ManagerPages;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace WinGetStore.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly List<(string Tag, Type Page)> _pages = new()
        {
            ("Home", typeof(ManagerPage)),
            ("Library", typeof(InstallingPage)),
            ("Settings", typeof(SettingsPage))
        };

        public MainPage()
        {
            InitializeComponent();
            SearchBoxHolder.RegisterPropertyChangedCallback(Slot.IsStretchProperty, new DependencyPropertyChangedCallback(OnIsStretchProperty));
            NavigationView.RegisterPropertyChangedCallback(muxc.NavigationView.IsBackButtonVisibleProperty, new DependencyPropertyChangedCallback(OnIsBackButtonVisibleChanged));
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush")) { BackdropMaterial.SetApplyToRootOrPageBackground(this, true); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current?.SetTitleBar(DragRegion);
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            //AppTitleText.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SetTitleBar(null);
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed -= System_BackPressed; }
            SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateAppTitle(sender);
        }

        public string GetAppTitleFromSystem => Package.Current.DisplayName;

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
            // entries in the backstack.
            Type PreNavPageType = NavigationViewFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Equals(PreNavPageType, _page))
            {
                _ = NavigationViewFrame.Navigate(_page, vs, TransitionInfo);
            }
        }

        private void NavigationView_BackRequested(muxc.NavigationView sender, muxc.NavigationViewBackRequestedEventArgs args) => _ = TryGoBack();

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string NavItemTag = args.SelectedItemContainer.Tag.ToString();
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
                (NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Compact ||
                 NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
            { return false; }

            NavigationViewFrame.GoBack();
            return true;
        }

        private void On_Navigated(object _, NavigationEventArgs e)
        {
            NavigationView.IsBackEnabled = NavigationViewFrame.CanGoBack;
            NavigationView.IsBackButtonVisible = NavigationViewFrame.CanGoBack
                ? muxc.NavigationViewBackButtonVisible.Visible
                : muxc.NavigationViewBackButtonVisible.Collapsed;
            if (NavigationViewFrame.SourcePageType != null)
            {
                (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                if (item.Tag != null)
                {
                    muxc.NavigationViewItem SelectedItem = NavigationView.MenuItems
                        .OfType<muxc.NavigationViewItem>()
                        .FirstOrDefault(n => n.Tag.Equals(item.Tag))
                            ?? NavigationView.FooterMenuItems
                                .OfType<muxc.NavigationViewItem>()
                                .FirstOrDefault(n => n.Tag.Equals(item.Tag));
                    NavigationView.SelectedItem = SelectedItem;
                }
            }
        }

        private void NavigationViewControl_PaneClosing(muxc.NavigationView sender, muxc.NavigationViewPaneClosingEventArgs args)
        {
            UpdateLeftPaddingColumn();
        }

        private void NavigationViewControl_PaneOpening(muxc.NavigationView sender, object args)
        {
            UpdateLeftPaddingColumn();
        }

        private void UpdateLeftPaddingColumn()
        {
            LeftPaddingColumn.Width = NavigationView.PaneDisplayMode == muxc.NavigationViewPaneDisplayMode.Top
                ? NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                    ? new GridLength(48) : new GridLength(0)
                    : NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal
                        ? NavigationView.IsPaneOpen ? new GridLength(72)
                        : NavigationView.IsPaneToggleButtonVisible
                            ? NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                            ? new GridLength(88) : new GridLength(48)
                                : NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                                ? new GridLength(48) : new GridLength(0)
                                    : NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                                    ? new GridLength(48) : new GridLength(0);
        }

        private void NavigationViewControl_DisplayModeChanged(muxc.NavigationView sender, muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void UpdateAppTitleIcon()
        {
            AppTitleIcon.Margin = SearchBoxHolder.IsStretch
                && NavigationView.PaneDisplayMode != muxc.NavigationViewPaneDisplayMode.Top
                && NavigationView.DisplayMode != muxc.NavigationViewDisplayMode.Minimal
                    ? NavigationView.IsBackButtonVisible == muxc.NavigationViewBackButtonVisible.Visible
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(24.5, 0, 24, 0)
                    : NavigationView.IsBackButtonVisible == muxc.NavigationViewBackButtonVisible.Visible
                        || (NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal
                            && NavigationView.PaneDisplayMode != muxc.NavigationViewPaneDisplayMode.Top
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

        private void System_BackPressed(object sender, BackPressedEventArgs e)
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
                NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text));
            }
        }
    }
}
