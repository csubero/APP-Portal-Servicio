using PortalAPI.Contracts;
using PortalServicio.Configuration;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            Types.SPCINCIDENT_CONTROLOPTION control = (Types.SPCINCIDENT_CONTROLOPTION)value;
            switch (control)
            {
                case Types.SPCINCIDENT_CONTROLOPTION.Finalizado: //En realidad es sin programar pero dado que este estado no se encuentra se utilizara este en su lugar
                    return Color.Red;
                case Types.SPCINCIDENT_CONTROLOPTION.Reproceso:
                    return Color.MediumVioletRed;
                case Types.SPCINCIDENT_CONTROLOPTION.Reprogramacion:
                    return Color.CadetBlue;
                case Types.SPCINCIDENT_CONTROLOPTION.Programado:
                    return Color.Green;
                case Types.SPCINCIDENT_CONTROLOPTION.EsperandoRevision:
                    return Color.Yellow;
                case Types.SPCINCIDENT_CONTROLOPTION.PendienteCotizar:
                    return Color.Blue;
                default:
                    return Color.Black;
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}