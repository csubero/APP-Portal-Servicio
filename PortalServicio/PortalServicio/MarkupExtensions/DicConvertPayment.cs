using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertPayment : IValueConverter
    {
        public object Convert(object value, Type targetType,
                           object parameter, CultureInfo culture)
        {
            switch ((Types.SPCINCIDENT_PAYMENTOPTION)value)
            {
                case Types.SPCINCIDENT_PAYMENTOPTION.Anticipo:
                    return 0;
                case Types.SPCINCIDENT_PAYMENTOPTION.Informativo:
                    return 1;
                case Types.SPCINCIDENT_PAYMENTOPTION.Verificacion:
                    return 2;
                case Types.SPCINCIDENT_PAYMENTOPTION.Garantia:
                    return 3;
                case Types.SPCINCIDENT_PAYMENTOPTION.Retiro:
                    return 4;
                case Types.SPCINCIDENT_PAYMENTOPTION.NA:
                    return 5;
                case Types.SPCINCIDENT_PAYMENTOPTION.Adelanto:
                    return 6;
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
                    return Types.SPCINCIDENT_PAYMENTOPTION.Anticipo;
                case 1:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Informativo;
                case 2:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Verificacion;
                case 3:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Garantia;
                case 4:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Retiro;
                case 5:
                    return Types.SPCINCIDENT_PAYMENTOPTION.NA;
                case 6:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Adelanto;               
                default:
                    return Types.SPCINCIDENT_PAYMENTOPTION.Undefined;
            }
        }
    }
}
