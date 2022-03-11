using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertLeadState : IValueConverter
    {
        public object Convert(object value, Type targetType,
                           object parameter, CultureInfo culture)
        {
            switch ((Types.SPCSERVTICKET_LEADSTATE)value)
            {
                case Types.SPCSERVTICKET_LEADSTATE.Ok:
                    return 0;
                case Types.SPCSERVTICKET_LEADSTATE.Reg:
                    return 1;
                case Types.SPCSERVTICKET_LEADSTATE.Mal:
                    return 2;
                default:
                    return -1;
            }         
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_LEADSTATE.Ok;
                case 1:
                    return Types.SPCSERVTICKET_LEADSTATE.Reg;
                case 2:
                    return Types.SPCSERVTICKET_LEADSTATE.Mal;
                default:
                    return Types.SPCSERVTICKET_LEADSTATE.Undefined;
            }          
        }
    }
}
