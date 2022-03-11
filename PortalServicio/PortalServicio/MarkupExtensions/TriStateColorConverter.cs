using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class TriStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int state = 0;
            try
            {
                state = (int)value;
            }catch(Exception)
            {
                state = (bool)value ? 0 : 2;
            }
            switch (state)
            {
                case 0: //Available
                    return Color.Green;
                case 1: //Busy
                    return Color.Yellow;
                case 2: //Unavailable
                    return Color.Red;
                default:
                    return Color.Blue; //Default or not selected
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
