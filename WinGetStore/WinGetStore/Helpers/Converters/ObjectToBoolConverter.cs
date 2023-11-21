using Microsoft.Toolkit.Uwp.UI.Converters;

namespace WinGetStore.Helpers.Converters
{
    /// <summary>
    /// This class converts a boolean value into a Opacity value.
    /// </summary>
    public class BoolToOpacityConverter : BoolToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolToOpacityConverter"/> class.
        /// </summary>
        public BoolToOpacityConverter()
        {
            TrueValue = 1.0;
            FalseValue = 0.0;
        }
    }
}
