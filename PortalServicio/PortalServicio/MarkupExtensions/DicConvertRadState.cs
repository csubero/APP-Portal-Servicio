using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertRadState : IValueConverter
    {
        public object Convert(object value, Type targetType,
                           object parameter, CultureInfo culture)
        {
            switch ((Types.SPCSERVTICKET_RADSTATE)value)
            {
                case Types.SPCSERVTICKET_RADSTATE.Cumple:
                    return 0;
                case Types.SPCSERVTICKET_RADSTATE.NoCumple:
                    return 1;
                default:
                    return -1;
            }
            //if (value is Types.SPCSERVTICKET_RADSTATE)
            //{
            //    if ((Types.SPCSERVTICKET_RADSTATE)value == Types.SPCSERVTICKET_RADSTATE.Undefined)
            //        return null;
            //    return ((XRayChecklistMPageViewModel)((ContentPage)parameter).BindingContext).Dic_RadStateList.Where(e => e.Value.Equals((Types.SPCSERVTICKET_RADSTATE)value)).FirstOrDefault();
            //}
            //return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value != null)
                switch ((int)value)
                {
                    case 0:
                        return Types.SPCSERVTICKET_RADSTATE.Cumple;
                    case 1:
                        return Types.SPCSERVTICKET_RADSTATE.NoCumple;
                    default:
                        return Types.SPCSERVTICKET_RADSTATE.Undefined;
                }
            return Types.SPCSERVTICKET_RADSTATE.Undefined;
            //if (value is KeyValuePair<string, Types.SPCSERVTICKET_RADSTATE>)
            //{
            //    if (value == null)
            //        return Types.SPCSERVTICKET_RADSTATE.Undefined;
            //    return ((KeyValuePair<string, Types.SPCSERVTICKET_RADSTATE>)value).Value;
            //}
            //return value;
        }
    }
}
