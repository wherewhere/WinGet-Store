using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinGetStore.Controls
{
    /// <summary>
    /// This is the base control to create consistent settings experiences, inline with the Windows 11 design language.
    /// A <see cref="SettingsCard"/> can also be hosted within a <see cref="SettingsExpander"/>.
    /// </summary>
    [TemplatePart(Name = HeaderIconPresenterHolder, Type = typeof(Viewbox))]
    [TemplatePart(Name = ContentPresenter, Type = typeof(ContentPresenter))]
    [TemplateVisualState(Name = NormalState, GroupName = CommonStates)]
    [TemplateVisualState(Name = PointerOverState, GroupName = CommonStates)]
    [TemplateVisualState(Name = PressedState, GroupName = CommonStates)]
    [TemplateVisualState(Name = DisabledState, GroupName = CommonStates)]
    [TemplateVisualState(Name = ActionIconVisibleState, GroupName = ActionIconVisibilityGroup)]
    [TemplateVisualState(Name = ActionIconCollapsedState, GroupName = ActionIconVisibilityGroup)]
    public partial class SettingsCard : ButtonBase
    {
        private const string CommonStates = "CommonStates";
        private const string NormalState = "Normal";
        private const string PointerOverState = "PointerOver";
        private const string PressedState = "Pressed";
        private const string DisabledState = "Disabled";

        private const string ActionIconVisibilityGroup = "ActionIconVisibilityGroup";
        private const string ActionIconVisibleState = "ActionIconVisible";
        private const string ActionIconCollapsedState = "ActionIconCollapsed";

        private const string ContentPresenter = "PART_ContentPresenter";
        private const string HeaderIconPresenterHolder = "PART_HeaderIconPresenterHolder";

        /// <summary>
        /// Creates a new instance of the <see cref="SettingsCard"/> class.
        /// </summary>
        public SettingsCard()
        {
            DefaultStyleKey = typeof(SettingsCard);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IsEnabledChanged -= OnIsEnabledChanged;
            OnButtonIconChanged();
            OnHeaderChanged();
            OnHeaderIconChanged();
            OnDescriptionChanged();
            OnContentChanged();
            OnIsClickEnabledChanged();
            _ = VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
            RegisterAutomation();
            IsEnabledChanged += OnIsEnabledChanged;
        }

        private void RegisterAutomation()
        {
            if (Header is string headerString && headerString != string.Empty)
            {
                AutomationProperties.SetName(this, headerString);
                // We don't want to override an AutomationProperties.Name that is manually set, or if the Content basetype is of type ButtonBase (the ButtonBase.Content will be used then)
                if (Content is UIElement element
                    && string.IsNullOrEmpty(AutomationProperties.GetName(element))
                    && element is not ButtonBase or TextBlock)
                {
                    AutomationProperties.SetName(element, headerString);
                }
            }
        }

        private void EnableButtonInteraction()
        {
            DisableButtonInteraction();

            PointerEntered += Control_PointerEntered;
            PointerExited += Control_PointerExited;
            PointerCaptureLost += Control_PointerCaptureLost;
            PointerCanceled += Control_PointerCanceled;
            PreviewKeyDown += Control_PreviewKeyDown;
            PreviewKeyUp += Control_PreviewKeyUp;
        }

        private void DisableButtonInteraction()
        {
            PointerEntered -= Control_PointerEntered;
            PointerExited -= Control_PointerExited;
            PointerCaptureLost -= Control_PointerCaptureLost;
            PointerCanceled -= Control_PointerCanceled;
            PreviewKeyDown -= Control_PreviewKeyDown;
            PreviewKeyUp -= Control_PreviewKeyUp;
        }

        private void Control_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key is Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.Space or Windows.System.VirtualKey.GamepadA)
            {
                _ = VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        private void Control_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key is Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.Space or Windows.System.VirtualKey.GamepadA)
            {
                _ = VisualStateManager.GoToState(this, PressedState, true);
            }
        }

        public void Control_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            _ = VisualStateManager.GoToState(this, PointerOverState, true);
        }

        public void Control_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            _ = VisualStateManager.GoToState(this, NormalState, true);
        }

        private void Control_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            _ = VisualStateManager.GoToState(this, NormalState, true);
        }

        private void Control_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);
            _ = VisualStateManager.GoToState(this, NormalState, true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            //  e.Handled = true;
            if (IsClickEnabled)
            {
                base.OnPointerPressed(e);
                _ = VisualStateManager.GoToState(this, PressedState, true);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if (IsClickEnabled)
            {
                base.OnPointerReleased(e);
                _ = VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            OnContentChanged();
        }

        /// <summary>
        /// Creates AutomationPeer
        /// </summary>
        /// <returns>An automation peer for <see cref="SettingsCard"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SettingsCardAutomationPeer(this);
        }

        private void OnIsClickEnabledChanged()
        {
            OnButtonIconChanged();
            if (IsClickEnabled)
            {
                EnableButtonInteraction();
            }
            else
            {
                DisableButtonInteraction();
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ = VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
        }


        public void OnButtonIconChanged()
        {
            _ = VisualStateManager.GoToState(this, IsClickEnabled && ActionIcon != null ? "ActionIconVisible" : "ActionIconCollapsed", false);
        }

        public void OnHeaderIconChanged()
        {
            if (GetTemplateChild(HeaderIconPresenterHolder) is FrameworkElement headerIconPresenter)
            {
                headerIconPresenter.Visibility = HeaderIcon != null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void OnContentChanged()
        {
            if (GetTemplateChild(ContentPresenter) is FrameworkElement contentPresenter)
            {
                contentPresenter.Visibility = Content != null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void OnDescriptionChanged()
        {
            _ = VisualStateManager.GoToState(this, (Description is string @string ? string.IsNullOrEmpty(@string) : Description == null) ? "DescriptionCollapsed" : "DescriptionVisible", false);
        }

        public void OnHeaderChanged()
        {
            _ = VisualStateManager.GoToState(this, (Header is string @string ? string.IsNullOrEmpty(@string) : Header == null) ? "HeaderCollapsed" : "HeaderVisible", false);
        }
    }
}
