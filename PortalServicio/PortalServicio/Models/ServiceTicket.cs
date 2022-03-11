using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class ServiceTicket
    {
        #region Properties
        [ForeignKey(typeof(Incident))]
        public int IncidentId { get; set; }
        [ForeignKey(typeof(Subtype))]
        public int TypeId { get; set; }
        [ForeignKey(typeof(Currency))]
        public int MoneyCurrencyId { get; set; }

        #region Basic
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        [MaxLength(25),NotNull]
        public string TicketNumber { get; set; }      
        [MaxLength(1000)]
        public string Description { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Subtype Type { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Currency MoneyCurrency { get; set; }
        [MaxLength(15)]
        public string Code { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<MaterialYRepuesto> ProductsUsed { get; set; }
        public DateTime CreationDate { get; set; }
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string WorkDone { get; set; }
        [ManyToMany(typeof(ServiceTicketTechnician), CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<Technician> Technicians { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public bool HadLunch { get; set; }
        public bool FeedbackSubmitted { get; set; } 
        [Ignore]
        public bool IsNotFinished { get { return (Finished.Equals(new DateTime())); } }    
        public bool RequiresQuotation { get; set; }
        [MaxLength(25)]
        public string Email { get; set; }
        #endregion
        #region RX
        #region General
        [MaxLength(25)]
        public string RXGenModel { get; set; }
        [MaxLength(25)]
        public string RXGenSerial { get; set; }
        [MaxLength(25)]
        public string RXGenCreationDate { get; set; }
        public bool RXGenCleanArea { get; set; }
        [MaxLength(1000)]
        public string RXGenComments { get; set; }
        public Types.SPCSERVTICKET_VISITNUMBER RXGenVisitNumber { get; set; }
        public Types.SPCSERVTICKET_HWSTATE RXGenHWState { get; set; }
        #endregion
        #region Manteinance
        public bool RXMantCheckLabels { get; set; }
        public bool RXMantCheckInOutSystem { get; set; }
        public bool RXMantCheckIRFences { get; set; }
        public bool RXMantCheckControlElements { get; set; }
        public bool RXMantCheckEngineControl { get; set; }
        public bool RXMantCheckConveyorBelt { get; set; }
        public bool RXMantCheckEngineTraction { get; set; }
        public bool RXMantCheckRollers { get; set; }
        public bool RXMantCheckEmergencyStop { get; set; }
        public bool RXMantCheckInterlock { get; set; }
        public bool RXMantCheckVoltMonitor { get; set; }
        public bool RXMantCheckSecurityCircuit { get; set; }
        public bool RXMantCheckConditioningSystem { get; set; }
        public bool RXMantCheckOS { get; set; }
        public bool RXMantCheckXRCone { get; set; }
        public bool RXMantCheckLineAndDetectionModules { get; set; }
        public bool RXMantCheckConfiguration { get; set; }
        public bool RXMantCheckKeyboard { get; set; }
        public bool RXMantCheckMonitorConfiguration { get; set; }
        public bool RXMantCheckTwoWayMode { get; set; }
        public bool RXMantCheckRadiationIndicators { get; set; }
        #endregion
        #region Voltage
        public bool RXVoltCheckHaveUPS { get; set; }
        public bool RXVoltCheckIsolationTransformator { get; set; }
        public Types.SPCSERVTICKET_SCREENTYPE RXMantCheckScreenType { get; set; }
        public Types.SPCSERVTICKET_LEADSTATE RXMantLeadState { get; set; }
        [MaxLength(25)]
        public string RXVoltInVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGroundVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltUPSCapacity { get; set; }
        [MaxLength(25)]
        public string RXVoltUPSInVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltUPSGroundVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator1XROnVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator1XROffVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator1XROnAnodeVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator1XROnHighVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator2XROnVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator2XROffVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator2XROnAnodeVoltage { get; set; }
        [MaxLength(25)]
        public string RXVoltGenerator2XROnHighVoltage { get; set; }
        #endregion
        #region RadiationMetrics
        [MaxLength(25)]
        public string RXRadHWTrademark { get; set; }
        [MaxLength(25)]
        public string RXRadHWModel { get; set; }
        public DateTime RXRadHWCalibrationDate { get; set; }
        public DateTime RXRadHWCalibrationDueDate { get; set; }
        [MaxLength(25)]
        public string RXRadTunnelRadIn { get; set; }
        [MaxLength(25)]
        public string RXRadTunnelRadOut { get; set; }
        [MaxLength(25)]
        public string RXRadOperatorRad { get; set; }
        public Types.SPCSERVTICKET_RADSTATE RXRadRadiationState { get; set; }
        #endregion
        #region Software (Optional)
        [MaxLength(25)]
        public string RXSoftPhysicalDongleSerial { get; set; }
        [MaxLength(25)]
        public string RXSoftSoftwareDongleSerial { get; set; }
        [MaxLength(25)]
        public string RXSoftSoftwareVersion { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveSEN { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHITIP { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXPLORE { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHISPOT { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveIMS { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXACT { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHDA { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXPORT { get; set; }
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXTRAIN { get; set; }
        public Types.SPCSERVTICKET_TECHNOLOGY RXSoftTechnology { get; set; }
        #endregion
        #region Calibration       
        public bool RXCalType1 { get; set; }
        public bool RXCalType2 { get; set; }
        public bool RXCalType3 { get; set; }
        public bool RXCalType4 { get; set; }
        [MaxLength(25)]
        public string RXCalSteelPenetration { get; set; }
        [MaxLength(25)]
        public string RXCalWireResolution { get; set; }
        public Types.SPCSERVTICKET_RADSTATE RXCalCalibrationState { get; set; }
        #endregion
        #endregion
        #endregion     

        public bool IsFinancialReportPossible()
        {
            foreach (MaterialYRepuesto product in ProductsUsed)
                if (product.Treatment.Equals(Types.SPCMATERIAL_TREATMENTOPTION.Cotizar))
                    return true;
            return false;
        }

        public bool IsLegalizable()
        {
            foreach (MaterialYRepuesto product in ProductsUsed)
                if (product.Treatment.Equals(Types.SPCMATERIAL_TREATMENTOPTION.Facturar))
                    return true;
            return false;
        }   
    }
}
