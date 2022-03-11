using PortalAPI.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    class TreatmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Types.SPCMATERIAL_TREATMENTOPTION)
                switch ((Types.SPCMATERIAL_TREATMENTOPTION)value)
                {
                    case Types.SPCMATERIAL_TREATMENTOPTION.Facturar:
                        return 0;
                    case Types.SPCMATERIAL_TREATMENTOPTION.Cotizar:
                        return 1;
                    case Types.SPCMATERIAL_TREATMENTOPTION.Soporte:
                        return 2;
                    case Types.SPCMATERIAL_TREATMENTOPTION.Desmonte:
                        return 3;
                    default:
                        return -1;
                }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Types.SPCMATERIAL_TREATMENTOPTION.Undefined;
            switch ((int)value)
            {
                case 0:
                    return Types.SPCMATERIAL_TREATMENTOPTION.Facturar;
                case 1:
                    return Types.SPCMATERIAL_TREATMENTOPTION.Cotizar;
                case 2:
                    return Types.SPCMATERIAL_TREATMENTOPTION.Soporte;
                case 3:
                    return Types.SPCMATERIAL_TREATMENTOPTION.Desmonte;
                default:
                    return Types.SPCMATERIAL_TREATMENTOPTION.Undefined;
            }
        }
    }
}
