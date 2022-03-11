using PortalAPI.Contracts;
using PortalServicio.ViewModels;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class ConverterDestination : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, CultureInfo culture)
        {
            if (value is Types.SPCMATERIAL_DESTINATIONOPTION)
                if (((Types.SPCMATERIAL_DESTINATIONOPTION)value) == Types.SPCMATERIAL_DESTINATIONOPTION.Undefined)
                    return false;
                else
                    return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
