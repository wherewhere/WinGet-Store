using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinGetStore.Controls
{
    /// <summary>
    /// <see cref="StyleSelector"/> used by <see cref="SettingsExpander"/> to choose the proper <see cref="SettingsCard"/> container style (clickable or not).
    /// </summary>
    public partial class SettingsExpanderItemStyleSelector : StyleSelector
    {
        /// <summary>
        /// Gets or sets the default <see cref="Style"/> for <see cref="SettingsCard"/>.
        /// </summary>
        public Style DefaultStyle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Style"/> for <see cref="SettingsCard"/> when clickable.
        /// </summary>
        public Style ClickableStyle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Style"/> for <see cref="SettingsExpander"/>.
        /// </summary>
        public Style SettingsExpanderStyle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Style"/> for <see cref="Grid"/>.
        /// </summary>
        public Style GridStyle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Style"/> for <see cref="Border"/>.
        /// </summary>
        public Style BorderStyle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Style"/> for <see cref="StackPanel"/>.
        /// </summary>
        public Style StackPanelStyle { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsExpanderItemStyleSelector"/> class.
        /// </summary>
        public SettingsExpanderItemStyleSelector()
        {
        }

        /// <inheritdoc/>
        protected override Style SelectStyleCore(object item, DependencyObject container) =>
            container switch
            {
                SettingsCard card => card.IsClickEnabled ? ClickableStyle : DefaultStyle,
                SettingsExpander => SettingsExpanderStyle,
                Grid => GridStyle,
                Border => BorderStyle,
                StackPanel => StackPanelStyle,
                FrameworkElement element => element.Style,
                _ => null
            };
    }
}
