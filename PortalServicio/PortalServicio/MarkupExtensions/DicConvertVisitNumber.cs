using PortalAPI.Contracts;
using PortalServicio.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    class DicConvertVisitNumber : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is Types.SPCSERVTICKET_VISITNUMBER)
            {               
                switch ((Types.SPCSERVTICKET_VISITNUMBER)value)
                {
                    case Types.SPCSERVTICKET_VISITNUMBER.Undefined:
                        return -1;
                    case Types.SPCSERVTICKET_VISITNUMBER.Visita1:
                        return 0;
                    case Types.SPCSERVTICKET_VISITNUMBER.Visita2:
                        return 1;
                    case Types.SPCSERVTICKET_VISITNUMBER.Visita3:
                        return 2;
                    case Types.SPCSERVTICKET_VISITNUMBER.Visita4:
                        return 3;
                }
                // return Dic_VisitNumber.Where(e => e.Value.Equals((Types.SPCSERVTICKET_VISITNUMBER)value)).FirstOrDefault();
                //if ((Types.SPCSERVTICKET_VISITNUMBER)value == Types.SPCSERVTICKET_VISITNUMBER.Undefined)
                //    return null; 
                //return ((XRayChecklistMPageViewModel)((ContentPage)parameter).BindingContext).Dic_VisitNumberList.Where(e => e.Value.Equals((Types.SPCSERVTICKET_VISITNUMBER)value)).FirstOrDefault();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Types.SPCSERVTICKET_VISITNUMBER.Visita1;
                case 1:
                    return Types.SPCSERVTICKET_VISITNUMBER.Visita2;
                case 2:
                    return Types.SPCSERVTICKET_VISITNUMBER.Visita3;
                case 3:
                    return Types.SPCSERVTICKET_VISITNUMBER.Visita4;
                default:
                    return Types.SPCSERVTICKET_VISITNUMBER.Undefined;

            }
            //if (value is KeyValuePair<string, Types.SPCSERVTICKET_VISITNUMBER>)
            //{
            //    if (value == null)
            //        return Types.SPCSERVTICKET_VISITNUMBER.Undefined;
            //    return ((KeyValuePair<string, Types.SPCSERVTICKET_VISITNUMBER>)value).Value;
            //}
            //return value;
        }
    }
}
