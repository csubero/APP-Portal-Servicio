using PortalAPI.Contracts;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace PortalServicio.MarkupExtensions
{
    class DicLegalizationItemType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Types.SPCLEGALIZATIONITEM_TYPE)value)
            {
                case Types.SPCLEGALIZATIONITEM_TYPE.PhoneFaxBeeper:
                    return 0;
                case Types.SPCLEGALIZATIONITEM_TYPE.InternetCable:
                    return 1;
                case Types.SPCLEGALIZATIONITEM_TYPE.CarRent:
                    return 2;
                case Types.SPCLEGALIZATIONITEM_TYPE.Shipping:
                    return 3;
                case Types.SPCLEGALIZATIONITEM_TYPE.FuelOil:
                    return 4;
                case Types.SPCLEGALIZATIONITEM_TYPE.TicketsParkingToll:
                    return 5;
                case Types.SPCLEGALIZATIONITEM_TYPE.FoodHost:
                    return 6;
                case Types.SPCLEGALIZATIONITEM_TYPE.ManteinanceEquipment:
                    return 7;
                case Types.SPCLEGALIZATIONITEM_TYPE.CarManteinance:
                    return 8;
                case Types.SPCLEGALIZATIONITEM_TYPE.EmployeeAttending:
                    return 9;
                case Types.SPCLEGALIZATIONITEM_TYPE.ClientAttending:
                    return 10;
                case Types.SPCLEGALIZATIONITEM_TYPE.Courier:
                    return 11;
                case Types.SPCLEGALIZATIONITEM_TYPE.PaperAndOfficeTools:
                    return 12;
                case Types.SPCLEGALIZATIONITEM_TYPE.Surveillance:
                    return 13;
                case Types.SPCLEGALIZATIONITEM_TYPE.Cleaning:
                    return 14;
                case Types.SPCLEGALIZATIONITEM_TYPE.Transport:
                    return 15;
                case Types.SPCLEGALIZATIONITEM_TYPE.Advertising:
                    return 16;
                case Types.SPCLEGALIZATIONITEM_TYPE.TenderProject:
                    return 17;
                case Types.SPCLEGALIZATIONITEM_TYPE.ProfessionalServices:
                    return 18;
                case Types.SPCLEGALIZATIONITEM_TYPE.VisaOrTravelTaxes:
                    return 19;
                case Types.SPCLEGALIZATIONITEM_TYPE.Materials:
                    return 20;
                case Types.SPCLEGALIZATIONITEM_TYPE.MinorTools:
                    return 21;
                case Types.SPCLEGALIZATIONITEM_TYPE.PersonalTraining:
                    return 22;
                case Types.SPCLEGALIZATIONITEM_TYPE.NoDeductibleExpenses:
                    return 23;
                case Types.SPCLEGALIZATIONITEM_TYPE.EmployeeTaxes:
                    return 24;
                case Types.SPCLEGALIZATIONITEM_TYPE.WarrantyEquipmentRepairing:
                    return 25;
                case Types.SPCLEGALIZATIONITEM_TYPE.CarTraveling:
                    return 26;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return Types.SPCLEGALIZATIONITEM_TYPE.PhoneFaxBeeper;
                case 1:
                    return Types.SPCLEGALIZATIONITEM_TYPE.InternetCable;
                case 2:
                    return Types.SPCLEGALIZATIONITEM_TYPE.CarRent;
                case 3:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Shipping;
                case 4:
                    return Types.SPCLEGALIZATIONITEM_TYPE.FuelOil;
                case 5:
                    return Types.SPCLEGALIZATIONITEM_TYPE.TicketsParkingToll;
                case 6:
                    return Types.SPCLEGALIZATIONITEM_TYPE.FoodHost;
                case 7:
                    return Types.SPCLEGALIZATIONITEM_TYPE.ManteinanceEquipment;
                case 8:
                    return Types.SPCLEGALIZATIONITEM_TYPE.CarManteinance;
                case 9:
                    return Types.SPCLEGALIZATIONITEM_TYPE.EmployeeAttending;
                case 10:
                    return Types.SPCLEGALIZATIONITEM_TYPE.ClientAttending;
                case 11:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Courier;
                case 12:
                    return Types.SPCLEGALIZATIONITEM_TYPE.PaperAndOfficeTools;
                case 13:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Surveillance;
                case 14:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Cleaning;
                case 15:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Transport;
                case 16:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Advertising;
                case 17:
                    return Types.SPCLEGALIZATIONITEM_TYPE.TenderProject;
                case 18:
                    return Types.SPCLEGALIZATIONITEM_TYPE.ProfessionalServices;
                case 19:
                    return Types.SPCLEGALIZATIONITEM_TYPE.VisaOrTravelTaxes;
                case 20:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Materials;
                case 21:
                    return Types.SPCLEGALIZATIONITEM_TYPE.MinorTools;
                case 22:
                    return Types.SPCLEGALIZATIONITEM_TYPE.PersonalTraining;
                case 23:
                    return Types.SPCLEGALIZATIONITEM_TYPE.NoDeductibleExpenses;
                case 24:
                    return Types.SPCLEGALIZATIONITEM_TYPE.EmployeeTaxes;
                case 25:
                    return Types.SPCLEGALIZATIONITEM_TYPE.WarrantyEquipmentRepairing;
                case 26:
                    return Types.SPCLEGALIZATIONITEM_TYPE.CarTraveling;
                default:
                    return Types.SPCLEGALIZATIONITEM_TYPE.Undefined;
            }
        }
    }
}
