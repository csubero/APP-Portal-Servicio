using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    public class DicConvertProcessType : IValueConverter
    {
        public object Convert(object value, Type targetType,
                      object parameter, CultureInfo culture)
        {
            switch ((Types.SPCEXTRAEQUIPMENT_PROCESSTYPE)value)
            {
                case Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Order:
                    return 0;
                case Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Offer:
                    return 1;
                case Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Free:
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
                    return Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Order;
                case 1:
                    return Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Offer;
                case 2:
                    return Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Free;
                default:
                    return Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Undefined;
            }
        }
    }
}
