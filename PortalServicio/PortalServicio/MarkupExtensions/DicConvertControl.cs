using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertControl :IValueConverter
    {
        public object Convert(object value, Type targetType,
                          object parameter, CultureInfo culture)
        {
            switch ((Types.SPCINCIDENT_CONTROLOPTION)value)
            {
                case Types.SPCINCIDENT_CONTROLOPTION.Finalizado:
                    return 0;
                case Types.SPCINCIDENT_CONTROLOPTION.Reproceso:
                    return 1;
                case Types.SPCINCIDENT_CONTROLOPTION.Reprogramacion:
                    return 2;
                case Types.SPCINCIDENT_CONTROLOPTION.Programado:
                    return 3;
                case Types.SPCINCIDENT_CONTROLOPTION.EsperandoRevision:
                    return 4;
                case Types.SPCINCIDENT_CONTROLOPTION.PendienteCotizar:
                    return 5;               
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
                    return Types.SPCINCIDENT_CONTROLOPTION.Finalizado;
                case 1:
                    return Types.SPCINCIDENT_CONTROLOPTION.Reproceso;
                case 2:
                    return Types.SPCINCIDENT_CONTROLOPTION.Reprogramacion;
                case 3:
                    return Types.SPCINCIDENT_CONTROLOPTION.Programado;
                case 4:
                    return Types.SPCINCIDENT_CONTROLOPTION.EsperandoRevision;
                case 5:
                    return Types.SPCINCIDENT_CONTROLOPTION.PendienteCotizar;             
                default:
                    return Types.SPCINCIDENT_CONTROLOPTION.Undefined;
            }
        }
    }
}
