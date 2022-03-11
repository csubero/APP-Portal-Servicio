using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class IsNotNullObject : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {          
                return !(value is null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
