using CommunityToolkit.WinUI.Converters;

namespace WinGetStore.Helpers.Converters
{
    /// <summary>
    /// This class converts a object into a Boolean value (if the value is null returns a false value).
    /// </summary>
    public partial class ObjectToBoolConverter : EmptyObjectToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectToBoolConverter"/> class.
        /// </summary>
        public ObjectToBoolConverter()
        {
            NotEmptyValue = true;
            EmptyValue = false;
        }
    }
}
