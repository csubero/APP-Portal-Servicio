using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicLegalizationType : IValueConverter
    {
        public object Convert(object value, Type targetType,
                       object parameter, CultureInfo culture)
        {
            switch ((Types.SPCLEGALIZATION_TYPE)value)
            {
                case Types.SPCLEGALIZATION_TYPE.CajaChica:
                    return 0;
                case Types.SPCLEGALIZATION_TYPE.GastosViaje:
                    return 1;
                case Types.SPCLEGALIZATION_TYPE.Transferencia:
                    return 2;
                case Types.SPCLEGALIZATION_TYPE.TarjetaEmpresarial:
                    return 3;
                default:
                    return -1;
            }
            //return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value != null)
                switch ((int)value)
                {
                    case 0:
                        return Types.SPCLEGALIZATION_TYPE.CajaChica;
                    case 1:
                        return Types.SPCLEGALIZATION_TYPE.GastosViaje;
                    case 2:
                        return Types.SPCLEGALIZATION_TYPE.Transferencia;
                    case 3:
                        return Types.SPCLEGALIZATION_TYPE.TarjetaEmpresarial;
                    default:
                        return Types.SPCLEGALIZATION_TYPE.Undefined;
                }
            return Types.SPCLEGALIZATION_TYPE.Undefined;
        }
    }
}
