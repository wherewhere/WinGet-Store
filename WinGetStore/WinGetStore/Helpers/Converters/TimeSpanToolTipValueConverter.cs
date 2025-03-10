using System;
using Windows.UI.Xaml.Data;

namespace WinGetStore.Helpers.Converters
{
    public partial class TimeSpanToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time = value switch
            {
                TimeSpan timeSpan => timeSpan,
                string @string => TimeSpan.Parse(@string),
                _ => TimeSpan.FromMinutes(System.Convert.ToDouble(value)),
            };
            return ConverterTools.Convert(time.ToString(), targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            else
            {
                TimeSpan timeSpan = TimeSpan.Parse(value.ToString());
                return targetType == typeof(TimeSpan) ? timeSpan : ConverterTools.Convert(timeSpan.TotalMinutes, targetType);
            }
        }
    }
}
