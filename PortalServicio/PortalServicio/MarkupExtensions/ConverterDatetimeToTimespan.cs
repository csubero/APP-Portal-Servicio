using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class ConverterDatetimeToTimespan : IValueConverter
    {
        DateTime _SavedValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                _SavedValue = ((DateTime)value);
                return ((DateTime)value).TimeOfDay;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return default(DateTime);
            TimeSpan time = (TimeSpan)value;
            return new DateTime(_SavedValue.Year, _SavedValue.Month, _SavedValue.Day, time.Hours, time.Minutes, time.Seconds);
        }
    }
}
