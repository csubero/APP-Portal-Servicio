using System;

namespace PortalAPI.Contracts
{
    public static class Types
    {
        #region CRM Enumerations
        public enum SPCMATERIAL_TREATMENTOPTION
        {
            Undefined = 0,
            Facturar = 100000000,
            Cotizar,
            Garantía,
            Soporte,
            Desmonte
        };

        public enum SPCMATERIAL_DESTINATIONOPTION
        {
            Undefined = 0,
            Cliente = 100000000,
            Taller,
            Bodega
        };

        public enum SPCCLIENT_REPORTTYPEOPTION
        {
            Undefined = 0,
            Normal = 100000000,
            ContableSimple,
            ContableFull
        }

        public enum SPCINCIDENT_PAYMENTOPTION
        {
            Undefined = 0,
            Anticipo = 100000000,
            Informativo,
            Verificacion,
            Garantia,
            Retiro,
            NA,
            Adelanto
        }

        public enum SPCINCIDENT_CONTROLOPTION
        {
            Undefined = 0,
            Finalizado = 100000000,
            Reproceso,
            Reprogramacion,
            Programado,
            EsperandoRevision,
            PendienteCotizar
        }

        public enum SPCSERVTICKET_HWSTATE
        {
            Undefined = 0,
            Normal = 100000000,
            Falla
        }

        public enum SPCSERVTICKET_LEADSTATE
        {
            Undefined = 0,
            Ok = 100000000,
            Reg,
            Mal
        }

        public enum SPCSERVTICKET_SCREENTYPE
        {
            Undefined = 0,
            CRT = 100000000,
            LCD
        }

        public enum SPCSERVTICKET_RADSTATE
        {
            Undefined = 0,
            Cumple = 100000000,
            NoCumple
        }

        public enum SPCSERVTICKET_VISITNUMBER
        {
            Undefined = 0,
            Visita1 = 100000000,
            Visita2,
            Visita3,
            Visita4
        }

        public enum SPCSERVTICKET_POSSESIONSTATE
        {
            Undefined = 0,
            Enabled = 100000000,
            Disabled,
            NotHave
        }

        public enum SPCSERVTICKET_TECHNOLOGY
        {
            Undefined = 0,
            HiTrax = 100000000,
            SiProx
        }

        public enum SPCTECHNICIAN_CATEGORY
        {
            Undefined = 0,
            Tecnico = 100000000,
            Ayudante,
            Ingeniero,
            JefeTecnico
        }

        public enum SPCLEGALIZATION_TYPE
        {
            Undefined = 0,
            CajaChica = 100000000,
            GastosViaje,
            Transferencia,
            TarjetaEmpresarial
        }

        public enum SPCLEGALIZATIONITEM_TYPE
        {
            Undefined = 0,
            PhoneFaxBeeper = 100000000,
            InternetCable,
            CarRent,
            Shipping,
            FuelOil,
            TicketsParkingToll,
            FoodHost,
            ManteinanceEquipment,
            CarManteinance,
            EmployeeAttending,
            ClientAttending,
            Courier,
            PaperAndOfficeTools,
            Surveillance,
            Cleaning,
            Transport,
            Advertising,
            TenderProject,
            ProfessionalServices,
            VisaOrTravelTaxes,
            Materials,
            MinorTools,
            PersonalTraining,
            NoDeductibleExpenses,
            EmployeeTaxes,
            WarrantyEquipmentRepairing,
            CarTraveling
        }

        public enum SPCLEGALIZATION_SIGNSTATE
        {
            Unsigned = 0,
            SignedOwner,
            SignedManager,
            SignedFinancial
        }

        public enum SPCCDT_PROJECTSTATE
        {
            Undefined = 0,
            InTime = 100000000,
            NearToDeadline,
            Overdue,
            Paused
        }

        public enum SPCEXTRAEQUIPMENT_PROCESSTYPE
        {
            Undefined = 0,
            Order = 100000000,
            Offer,
            Free
        }

        public enum CRUDOperation
        {
            Create,
            Retrieve,
            Update,
            Delete
        }
        #endregion
    }
}