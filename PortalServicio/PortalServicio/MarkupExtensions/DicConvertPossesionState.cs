using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertPossesionState : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, CultureInfo culture)
        {
            if (value is Types.SPCSERVTICKET_POSSESIONSTATE)
                switch ((Types.SPCSERVTICKET_POSSESIONSTATE)value)
                {
                    case Types.SPCSERVTICKET_POSSESIONSTATE.Enabled:
                        return 0;
                    case Types.SPCSERVTICKET_POSSESIONSTATE.NotHave:
                        return 2;
                    case Types.SPCSERVTICKET_POSSESIONSTATE.Disabled:
                        return 1;
                    default:
                        return -1;
                }
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value == null)
                return Types.SPCSERVTICKET_POSSESIONSTATE.Undefined;
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_POSSESIONSTATE.Enabled;
                case 1:
                    return Types.SPCSERVTICKET_POSSESIONSTATE.Disabled;
                case 2:
                    return Types.SPCSERVTICKET_POSSESIONSTATE.NotHave;
                default:
                    return Types.SPCSERVTICKET_POSSESIONSTATE.Undefined;
            }
        }
    }
}
