using PortalAPI.Contracts;
using PortalServicio.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertHWState : IValueConverter
    {
        public object Convert(object value, Type targetType,
                             object parameter, CultureInfo culture)
        {
            if (value is Types.SPCSERVTICKET_HWSTATE)
                switch ((Types.SPCSERVTICKET_HWSTATE)value)
                {
                    case Types.SPCSERVTICKET_HWSTATE.Normal:
                        return 0;
                    case Types.SPCSERVTICKET_HWSTATE.Falla:
                        return 1;
                    default:
                        return -1;
                }
            //{
            //    if ((Types.SPCSERVTICKET_HWSTATE)value == Types.SPCSERVTICKET_HWSTATE.Undefined)
            //        return null;
            //    return ((XRayChecklistMPageViewModel)((ContentPage)parameter).BindingContext).Dic_HWStateList.Where(e => e.Value.Equals((Types.SPCSERVTICKET_HWSTATE)value)).FirstOrDefault();
            //}
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_HWSTATE.Normal;
                case 1:
                    return Types.SPCSERVTICKET_HWSTATE.Falla;
                default:
                    return Types.SPCSERVTICKET_HWSTATE.Undefined;
            }
            //if (value is KeyValuePair<string, Types.SPCSERVTICKET_HWSTATE>)
            //{
            //    if (value == null)
            //        return Types.SPCSERVTICKET_HWSTATE.Undefined;
            //    return ((KeyValuePair<string, Types.SPCSERVTICKET_HWSTATE>)value).Value;
            //}
            //return value;
        }
    }
}
