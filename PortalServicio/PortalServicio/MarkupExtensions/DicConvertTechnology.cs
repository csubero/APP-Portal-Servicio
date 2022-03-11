using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertTechnology : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            if (value == null)
                return -1;
            switch ((Types.SPCSERVTICKET_TECHNOLOGY)value)
            {
                case Types.SPCSERVTICKET_TECHNOLOGY.HiTrax:
                    return 0;
                case Types.SPCSERVTICKET_TECHNOLOGY.SiProx:
                    return 1;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value == null)
                return Types.SPCSERVTICKET_TECHNOLOGY.Undefined;
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_TECHNOLOGY.HiTrax;
                case 1:
                    return Types.SPCSERVTICKET_TECHNOLOGY.SiProx;
                default:
                    return Types.SPCSERVTICKET_TECHNOLOGY.Undefined;
            }
        }
    }
}
