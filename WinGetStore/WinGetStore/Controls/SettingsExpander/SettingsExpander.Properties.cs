using System.ComponentModel;
using Windows.UI.Xaml;

namespace WinGetStore.Controls
{
    public partial class SettingsExpander
    {
        #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(SettingsExpander),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Header.
        /// </summary>
        [Localizable(true)]
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region Description

        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(object),
                typeof(SettingsExpander),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        [Localizable(true)]
        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        #endregion

        #region ActionContent

        /// <summary>
        /// Identifies the <see cref="ActionContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActionContentProperty =
            DependencyProperty.Register(
                nameof(ActionContent),
                typeof(object),
                typeof(SettingsExpander),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content of a Setting.
        /// </summary>
        /// <returns>An object that contains the setting's content. The default is <see langword="null"/>.</returns>
        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        #endregion

        #region HeaderIcon

        /// <summary>
        /// Identifies the <see cref="HeaderIcon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderIconProperty =
            DependencyProperty.Register(
                nameof(HeaderIcon),
                typeof(object),
                typeof(SettingsExpander),
                new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the icon on the left.
        /// </summary>
        public object HeaderIcon
        {
            get => GetValue(HeaderIconProperty);
            set => SetValue(HeaderIconProperty, value);
        }

        #endregion

        #region ContentAlignment

        /// <summary>
        /// Identifies the <see cref="ContentAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register(
                nameof(ContentAlignment),
                typeof(ContentAlignment),
                typeof(SettingsExpander),
                new PropertyMetadata(ContentAlignment.Right));

        /// <summary>
        /// Gets or sets the alignment of the Content
        /// </summary>
        public ContentAlignment ContentAlignment
        {
            get => (ContentAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        #endregion

        #region ItemsHeader

        /// <summary>
        /// Identifies the <see cref="ItemsHeader"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsHeaderProperty =
            DependencyProperty.Register(
                nameof(ItemsHeader),
                typeof(UIElement),
                typeof(SettingsExpander),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ItemsFooter.
        /// </summary>
        public UIElement ItemsHeader
        {
            get => (UIElement)GetValue(ItemsHeaderProperty);
            set => SetValue(ItemsHeaderProperty, value);
        }

        #endregion

        #region ItemsFooter

        /// <summary>
        /// Identifies the <see cref="ItemsFooter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsFooterProperty =
            DependencyProperty.Register(
                nameof(ItemsFooter),
                typeof(UIElement),
                typeof(SettingsExpander),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ItemsFooter.
        /// </summary>
        public UIElement ItemsFooter
        {
            get => (UIElement)GetValue(ItemsFooterProperty);
            set => SetValue(ItemsFooterProperty, value);
        }

        #endregion

        #region IsExpanded

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(SettingsExpander),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the IsExpanded state.
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        #endregion
    }
}
