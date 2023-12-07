using System;
using Windows.UI.Xaml.Data;

namespace WinGetStore.Helpers.Converters
{
    public class TimeSpanToolTipValueConverter : IValueConverter
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
                if (targetType == typeof(TimeSpan))
                {
                    return timeSpan;
                }
                else
                {
                    return ConverterTools.Convert(timeSpan.TotalMinutes, targetType);
                }
            }
        }
    }
}
