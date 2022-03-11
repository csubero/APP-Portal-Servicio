using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DestinationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Types.SPCMATERIAL_DESTINATIONOPTION)
                switch ((Types.SPCMATERIAL_DESTINATIONOPTION)value)
                {
                    case Types.SPCMATERIAL_DESTINATIONOPTION.Cliente:
                        return 0;
                    case Types.SPCMATERIAL_DESTINATIONOPTION.Taller:
                        return 1;
                    case Types.SPCMATERIAL_DESTINATIONOPTION.Bodega:
                        return 2;
                    default:
                        return -1;
                }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Types.SPCMATERIAL_DESTINATIONOPTION.Undefined;
            switch ((int)value)
            {
                case 0:
                    return Types.SPCMATERIAL_DESTINATIONOPTION.Cliente;
                case 1:
                    return Types.SPCMATERIAL_DESTINATIONOPTION.Taller;
                case 2:
                    return Types.SPCMATERIAL_DESTINATIONOPTION.Bodega;
                default:
                    return Types.SPCMATERIAL_DESTINATIONOPTION.Undefined;
            }
        }
    }
}
