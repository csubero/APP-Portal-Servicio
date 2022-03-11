using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertScreenType : IValueConverter
    {
        public object Convert(object value, Type targetType,
                           object parameter, CultureInfo culture)
        {
            switch ((Types.SPCSERVTICKET_SCREENTYPE)value)
            {
                case Types.SPCSERVTICKET_SCREENTYPE.CRT:
                    return 0;
                case Types.SPCSERVTICKET_SCREENTYPE.LCD:
                    return 1;
                default:
                    return -1;
            }
            //if (value is Types.SPCSERVTICKET_SCREENTYPE)
            //{
            //    if ((Types.SPCSERVTICKET_SCREENTYPE)value == Types.SPCSERVTICKET_SCREENTYPE.Undefined)
            //        return null;
            //    return ((XRayChecklistMPageViewModel)((ContentPage)parameter).BindingContext).Dic_ScreenTypeList.Where(e => e.Value.Equals((Types.SPCSERVTICKET_SCREENTYPE)value)).FirstOrDefault();
            //}
            //return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_SCREENTYPE.CRT;
                case 1:
                    return Types.SPCSERVTICKET_SCREENTYPE.LCD;
                default:
                    return Types.SPCSERVTICKET_SCREENTYPE.Undefined;
            }
            //if (value is KeyValuePair<string, Types.SPCSERVTICKET_SCREENTYPE>)
            //{
            //    if (value == null)
            //        return Types.SPCSERVTICKET_SCREENTYPE.Undefined;
            //    return ((KeyValuePair<string, Types.SPCSERVTICKET_SCREENTYPE>)value).Value;
            //}
            //return value;
        }
    }
}
