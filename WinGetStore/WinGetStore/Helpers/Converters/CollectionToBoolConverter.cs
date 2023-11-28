using Microsoft.Toolkit.Uwp.UI.Converters;

namespace WinGetStore.Helpers.Converters
{
    /// <summary>
    /// This class converts a collection size to a Boolean value (if the value is null returns a false value).
    /// </summary>
    public class CollectionToBoolConverter : EmptyCollectionToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionToBoolConverter"/> class.
        /// </summary>
        public CollectionToBoolConverter()
        {
            NotEmptyValue = true;
            EmptyValue = false;
        }
    }
}
