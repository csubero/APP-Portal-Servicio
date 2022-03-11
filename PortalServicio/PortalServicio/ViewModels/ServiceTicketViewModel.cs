using PortalAPI.Contracts;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PortalServicio.ViewModels
{
    public class ServiceTicketViewModel : BaseViewModel
    {
        #region Properties
        #region ServiceTicket
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private int _IncidentId;
        public int IncidentId { get { return _IncidentId; } set { SetValue(ref _IncidentId, value); } }
        private int _TypeId;
        public int TypeId { get { return _TypeId; } set { SetValue(ref _TypeId, value); } }
        private int _MoneyCurrencyId;
        public int MoneyCurrencyId { get { return _MoneyCurrencyId; } set { SetValue(ref _MoneyCurrencyId, value); } }
        private string _TicketNumber;
        public string TicketNumber { get { return _TicketNumber; } set { SetValue(ref _TicketNumber, value); } }
        private string _Description;
        public string Description { get { return _Description; } set { SetValue(ref _Description, value); } }
        private SubtypeViewModel _Type;
        public SubtypeViewModel Type { get { return _Type; } set { SetValue(ref _Type, value); } }
        private CurrencyViewModel _MoneyCurrency;
        public CurrencyViewModel MoneyCurrency { get { return _MoneyCurrency; } set { SetValue(ref _MoneyCurrency, value); } }
        private string _Code;
        public string Code { get { return _Code; } set { SetValue(ref _Code, value); } }
        private ObservableCollection<MaterialYRepuestoViewModel> _ProductsUsed;
        public ObservableCollection<MaterialYRepuestoViewModel> ProductsUsed { get { return _ProductsUsed; } set { SetValue(ref _ProductsUsed, value); } }
        private DateTime _CreationDate;
        public DateTime CreationDate { get { return _CreationDate; } set { SetValue(ref _CreationDate, value); } }
        private string _Title;
        public string Title { get { return _Title; } set { SetValue(ref _Title, value); } }
       
        private string _WorkDone;
        public string WorkDone { get { return _WorkDone; } set { SetValue(ref _WorkDone, value); } }
        private TechnicianViewModel[] _Technicians;
        public TechnicianViewModel[] Technicians { get { return _Technicians; } set { SetValue(ref _Technicians, value); } }
        private DateTime _Started;
        public DateTime Started { get { return _Started; } set { SetValue(ref _Started, value); } }
        private DateTime _Finished;
        public DateTime Finished { get { return _Finished; } set { SetValue(ref _Finished, value); IsOpen = Finished.Equals(default(DateTime)); } }
        private Boolean _HadLunch;
        public Boolean HadLunch { get { return _HadLunch; } set { SetValue(ref _HadLunch, value); } }
        private Boolean _FeedBackSubmitted;
        public Boolean FeedbackSubmitted { get { return _FeedBackSubmitted; } set { SetValue(ref _FeedBackSubmitted, value); } }
        private bool _RequiresQuotation;
        public bool RequiresQuotation { get { return _RequiresQuotation; } set { SetValue(ref _RequiresQuotation, value); } }
        private string _Email;
        public string Email { get { return _Email; } set { SetValue(ref _Email, value); } }
        private bool _IsOpen;
        public bool IsOpen { get { return _IsOpen; } set { SetValue(ref _IsOpen, value); } }
        #endregion
        #region RX
        #region General
        private string _RXGenModel;
        public string RXGenModel { get { return _RXGenModel; } set { SetValue(ref _RXGenModel, value); } }
        private string _RXGenSerial;
        public string RXGenSerial { get { return _RXGenSerial; } set { SetValue(ref _RXGenSerial, value); } }
        public string RXGenCreationDate
        {
            get { return _RXGenCreationDateMonth + " " + _RXGenCreationDateYear; }
            set
            {
                if (value == null)
                    return;
                string[] values = value.Split(' ');
                if (values[0] != null)
                    RXGenCreationDateMonth = values[0];
                if (values[1] != null)
                    RXGenCreationDateYear = values[1];
            }
        }
        private string _RXGenCreationDateMonth;
        public string RXGenCreationDateMonth { get { return _RXGenCreationDateMonth; } set { SetValue(ref _RXGenCreationDateMonth, value); } }
        private string _RXGenCreationDateYear;
        public string RXGenCreationDateYear { get { return _RXGenCreationDateYear; } set { SetValue(ref _RXGenCreationDateYear, value); } }
        private bool _RXGenCleanArea;
        public bool RXGenCleanArea { get { return _RXGenCleanArea; } set { SetValue(ref _RXGenCleanArea, value); } }
        private string _RXGenComments;
        public string RXGenComments { get { return _RXGenComments; } set { SetValue(ref _RXGenComments, value); } }
        private Types.SPCSERVTICKET_VISITNUMBER _RXGenVisitNumber;
        public Types.SPCSERVTICKET_VISITNUMBER RXGenVisitNumber { get { return _RXGenVisitNumber; } set { SetValue(ref _RXGenVisitNumber, value); } }
        private Types.SPCSERVTICKET_HWSTATE _RXGenHWState;
        public Types.SPCSERVTICKET_HWSTATE RXGenHWState { get { return _RXGenHWState; } set { SetValue(ref _RXGenHWState, value); } }
        #endregion
        #region Manteinance
        private bool _RXMantCheckLabels;
        public bool RXMantCheckLabels { get { return _RXMantCheckLabels; } set { SetValue(ref _RXMantCheckLabels, value); } }
        private bool _RXMantCheckInOutSystem;
        public bool RXMantCheckInOutSystem { get { return _RXMantCheckInOutSystem; } set { SetValue(ref _RXMantCheckInOutSystem, value); } }
        private bool _RXMantCheckIRFences;
        public bool RXMantCheckIRFences { get { return _RXMantCheckIRFences; } set { SetValue(ref _RXMantCheckIRFences, value); } }
        private bool _RXMantCheckControlElements;
        public bool RXMantCheckControlElements { get { return _RXMantCheckControlElements; } set { SetValue(ref _RXMantCheckControlElements, value); } }
        private bool _RXMantCheckEngineControl;
        public bool RXMantCheckEngineControl { get { return _RXMantCheckEngineControl; } set { SetValue(ref _RXMantCheckEngineControl, value); } }
        private bool _RXMantCheckConveyorBelt;
        public bool RXMantCheckConveyorBelt { get { return _RXMantCheckConveyorBelt; } set { SetValue(ref _RXMantCheckConveyorBelt, value); } }
        private bool _RXMantCheckEngineTraction;
        public bool RXMantCheckEngineTraction { get { return _RXMantCheckEngineTraction; } set { SetValue(ref _RXMantCheckEngineTraction, value); } }
        private bool _RXMantCheckRollers;
        public bool RXMantCheckRollers { get { return _RXMantCheckRollers; } set { SetValue(ref _RXMantCheckRollers, value); } }
        private bool _RXMantCheckEmergencyStop;
        public bool RXMantCheckEmergencyStop { get { return _RXMantCheckEmergencyStop; } set { SetValue(ref _RXMantCheckEmergencyStop, value); } }
        private bool _RXMantCheckInterlock;
        public bool RXMantCheckInterlock { get { return _RXMantCheckInterlock; } set { SetValue(ref _RXMantCheckInterlock, value); } }
        private bool _RXMantCheckVoltMonitor;
        public bool RXMantCheckVoltMonitor { get { return _RXMantCheckVoltMonitor; } set { SetValue(ref _RXMantCheckVoltMonitor, value); } }
        private bool _RXMantCheckSecurityCircuit;
        public bool RXMantCheckSecurityCircuit { get { return _RXMantCheckSecurityCircuit; } set { SetValue(ref _RXMantCheckSecurityCircuit, value); } }
        private bool _RXMantCheckConditioningSystem;
        public bool RXMantCheckConditioningSystem { get { return _RXMantCheckConditioningSystem; } set { SetValue(ref _RXMantCheckConditioningSystem, value); } }
        private bool _RXMantCheckOS;
        public bool RXMantCheckOS { get { return _RXMantCheckOS; } set { SetValue(ref _RXMantCheckOS, value); } }
        private bool _RXMantCheckXRCone;
        public bool RXMantCheckXRCone { get { return _RXMantCheckXRCone; } set { SetValue(ref _RXMantCheckXRCone, value); } }
        private bool _RXMantCheckLineAndDetectionModules;
        public bool RXMantCheckLineAndDetectionModules { get { return _RXMantCheckLineAndDetectionModules; } set { SetValue(ref _RXMantCheckLineAndDetectionModules, value); } }
        private bool _RXMantCheckConfiguration;
        public bool RXMantCheckConfiguration { get { return _RXMantCheckConfiguration; } set { SetValue(ref _RXMantCheckConfiguration, value); } }
        private bool _RXMantCheckKeyboard;
        public bool RXMantCheckKeyboard { get { return _RXMantCheckKeyboard; } set { SetValue(ref _RXMantCheckKeyboard, value); } }
        private bool _RXMantCheckMonitorConfiguration;
        public bool RXMantCheckMonitorConfiguration { get { return _RXMantCheckMonitorConfiguration; } set { SetValue(ref _RXMantCheckMonitorConfiguration, value); } }
        private bool _RXMantCheckTwoWayMode;
        public bool RXMantCheckTwoWayMode { get { return _RXMantCheckTwoWayMode; } set { SetValue(ref _RXMantCheckTwoWayMode, value); } }
        private bool _RXMantCheckRadiationIndicators;
        public bool RXMantCheckRadiationIndicators { get { return _RXMantCheckRadiationIndicators; } set { SetValue(ref _RXMantCheckRadiationIndicators, value); } }
        private Types.SPCSERVTICKET_SCREENTYPE _RXMantCheckScreenType;
        public Types.SPCSERVTICKET_SCREENTYPE RXMantCheckScreenType { get { return _RXMantCheckScreenType; } set { SetValue(ref _RXMantCheckScreenType, value); } }
        private Types.SPCSERVTICKET_LEADSTATE _RXMantLeadState;
        public Types.SPCSERVTICKET_LEADSTATE RXMantLeadState { get { return _RXMantLeadState; } set { SetValue(ref _RXMantLeadState, value); } }
        #endregion
        #region Voltage
        private bool _RXVoltCheckHaveUPS;
        public bool RXVoltCheckHaveUPS { get { return _RXVoltCheckHaveUPS; } set { SetValue(ref _RXVoltCheckHaveUPS, value); } }
        private bool _RXVoltCheckIsolationTransformator;
        public bool RXVoltCheckIsolationTransformator { get { return _RXVoltCheckIsolationTransformator; } set { SetValue(ref _RXVoltCheckIsolationTransformator, value); } }
        private string _RXVoltInVoltage;
        public string RXVoltInVoltage { get { return _RXVoltInVoltage; } set { SetValue(ref _RXVoltInVoltage, value); } }
        private string _RXVoltGroundVoltage;
        public string RXVoltGroundVoltage { get { return _RXVoltGroundVoltage; } set { SetValue(ref _RXVoltGroundVoltage, value); } }
        private string _RXVoltUPSCapacity;
        public string RXVoltUPSCapacity { get { return _RXVoltUPSCapacity; } set { SetValue(ref _RXVoltUPSCapacity, value); } }
        private string _RXVoltUPSInVoltage;
        public string RXVoltUPSInVoltage { get { return _RXVoltUPSInVoltage; } set { SetValue(ref _RXVoltUPSInVoltage, value); } }
        private string _RXVoltUPSGroundVoltage;
        public string RXVoltUPSGroundVoltage { get { return _RXVoltUPSGroundVoltage; } set { SetValue(ref _RXVoltUPSGroundVoltage, value); } }
        private string _RXVoltGenerator1XROnVoltage;
        public string RXVoltGenerator1XROnVoltage { get { return _RXVoltGenerator1XROnVoltage; } set { SetValue(ref _RXVoltGenerator1XROnVoltage, value); } }
        private string _RXVoltGenerator1XROffVoltage;
        public string RXVoltGenerator1XROffVoltage { get { return _RXVoltGenerator1XROffVoltage; } set { SetValue(ref _RXVoltGenerator1XROffVoltage, value); } }
        private string _RXVoltGenerator1XROnAnodeVoltage;
        public string RXVoltGenerator1XROnAnodeVoltage { get { return _RXVoltGenerator1XROnAnodeVoltage; } set { SetValue(ref _RXVoltGenerator1XROnAnodeVoltage, value); } }
        private string _RXVoltGenerator1XROnHighVoltage;
        public string RXVoltGenerator1XROnHighVoltage { get { return _RXVoltGenerator1XROnHighVoltage; } set { SetValue(ref _RXVoltGenerator1XROnHighVoltage, value); } }
        private string _RXVoltGenerator2XROnVoltage;
        public string RXVoltGenerator2XROnVoltage { get { return _RXVoltGenerator2XROnVoltage; } set { SetValue(ref _RXVoltGenerator2XROnVoltage, value); } }
        private string _RXVoltGenerator2XROffVoltage;
        public string RXVoltGenerator2XROffVoltage { get { return _RXVoltGenerator2XROffVoltage; } set { SetValue(ref _RXVoltGenerator2XROffVoltage, value); } }
        private string _RXVoltGenerator2XROnAnodeVoltage;
        public string RXVoltGenerator2XROnAnodeVoltage { get { return _RXVoltGenerator2XROnAnodeVoltage; } set { SetValue(ref _RXVoltGenerator2XROnAnodeVoltage, value); } }
        private string _RXVoltGenerator2XROnHighVoltage;
        public string RXVoltGenerator2XROnHighVoltage { get { return _RXVoltGenerator2XROnHighVoltage; } set { SetValue(ref _RXVoltGenerator2XROnHighVoltage, value); } }
        #endregion
        #region RadiationMetrics
        private string _RXRadHWTrademark;
        public string RXRadHWTrademark { get { return _RXRadHWTrademark; } set { SetValue(ref _RXRadHWTrademark, value); } }
        private string _RXRadHWModel;
        public string RXRadHWModel { get { return _RXRadHWModel; } set { SetValue(ref _RXRadHWModel, value); } }
        private DateTime _RXRadHWCalibrationDate;
        public DateTime RXRadHWCalibrationDate { get { return _RXRadHWCalibrationDate; } set { SetValue(ref _RXRadHWCalibrationDate, value); } }
        private DateTime _RXRadHWCalibrationDueDate;
        public DateTime RXRadHWCalibrationDueDate { get { return _RXRadHWCalibrationDueDate; } set { SetValue(ref _RXRadHWCalibrationDueDate, value); } }
        private string _RXRadTunnelRadIn;
        public string RXRadTunnelRadIn { get { return _RXRadTunnelRadIn; } set { SetValue(ref _RXRadTunnelRadIn, value); } }
        private string _RXRadTunnelRadOut;
        public string RXRadTunnelRadOut { get { return _RXRadTunnelRadOut; } set { SetValue(ref _RXRadTunnelRadOut, value); } }
        private string _RXRadOperatorRad;
        public string RXRadOperatorRad { get { return _RXRadOperatorRad; } set { SetValue(ref _RXRadOperatorRad, value); } }
        private Types.SPCSERVTICKET_RADSTATE _RXRadRadiationState;
        public Types.SPCSERVTICKET_RADSTATE RXRadRadiationState { get { return _RXRadRadiationState; } set { SetValue(ref _RXRadRadiationState, value); } }
        #endregion
        #region Software (Optional)
        private string _RXSoftPhysicalDongleSerial;
        public string RXSoftPhysicalDongleSerial { get { return _RXSoftPhysicalDongleSerial; } set { SetValue(ref _RXSoftPhysicalDongleSerial, value); } }
        private string _RXSoftSoftwareDongleSerial;
        public string RXSoftSoftwareDongleSerial { get { return _RXSoftSoftwareDongleSerial; } set { SetValue(ref _RXSoftSoftwareDongleSerial, value); } }
        private string _RXSoftSoftwareVersion;
        public string RXSoftSoftwareVersion { get { return _RXSoftSoftwareVersion; } set { SetValue(ref _RXSoftSoftwareVersion, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveSEN;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveSEN { get { return _RXSoftHaveSEN; } set { SetValue(ref _RXSoftHaveSEN, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveHITIP;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHITIP { get { return _RXSoftHaveHITIP; } set { SetValue(ref _RXSoftHaveHITIP, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveXPLORE;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXPLORE { get { return _RXSoftHaveXPLORE; } set { SetValue(ref _RXSoftHaveXPLORE, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveHISPOT;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHISPOT { get { return _RXSoftHaveHISPOT; } set { SetValue(ref _RXSoftHaveHISPOT, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveIMS;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveIMS { get { return _RXSoftHaveIMS; } set { SetValue(ref _RXSoftHaveIMS, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveXACT;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXACT { get { return _RXSoftHaveXACT; } set { SetValue(ref _RXSoftHaveXACT, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveHDA;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveHDA { get { return _RXSoftHaveHDA; } set { SetValue(ref _RXSoftHaveHDA, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveXPORT;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXPORT { get { return _RXSoftHaveXPORT; } set { SetValue(ref _RXSoftHaveXPORT, value); } }
        private Types.SPCSERVTICKET_POSSESIONSTATE _RXSoftHaveXTRAIN;
        public Types.SPCSERVTICKET_POSSESIONSTATE RXSoftHaveXTRAIN { get { return _RXSoftHaveXTRAIN; } set { SetValue(ref _RXSoftHaveXTRAIN, value); } }
        private Types.SPCSERVTICKET_TECHNOLOGY _RXSoftTechnology;
        public Types.SPCSERVTICKET_TECHNOLOGY RXSoftTechnology { get { return _RXSoftTechnology; } set { SetValue(ref _RXSoftTechnology, value); } }
        #endregion
        #region Calibration
        private bool _RXCalType1;
        public bool RXCalType1 { get { return _RXCalType1; } set { SetValue(ref _RXCalType1, value); } }
        private bool _RXCalType2;
        public bool RXCalType2 { get { return _RXCalType2; } set { SetValue(ref _RXCalType2, value); } }
        private bool _RXCalType3;
        public bool RXCalType3 { get { return _RXCalType3; } set { SetValue(ref _RXCalType3, value); } }
        private bool _RXCalType4;
        public bool RXCalType4 { get { return _RXCalType4; } set { SetValue(ref _RXCalType4, value); } }
        private string _RXCalSteelPenetration;
        public string RXCalSteelPenetration { get { return _RXCalSteelPenetration; } set { SetValue(ref _RXCalSteelPenetration, value); } }
        private string _RXCalWireResolution;
        public string RXCalWireResolution { get { return _RXCalWireResolution; } set { SetValue(ref _RXCalWireResolution, value); } }
        private Types.SPCSERVTICKET_RADSTATE _RXCalCalibrationState;
        public Types.SPCSERVTICKET_RADSTATE RXCalCalibrationState { get { return _RXCalCalibrationState; } set { SetValue(ref _RXCalCalibrationState, value); } }
        #endregion
        #endregion
        #endregion

        #region Constructors
        public ServiceTicketViewModel(ServiceTicket serviceTicket)
        {
            if (serviceTicket == null)
                return;
            InternalId = serviceTicket.InternalId;
            SQLiteRecordId = serviceTicket.SQLiteRecordId;
            IncidentId = serviceTicket.IncidentId;
            MoneyCurrencyId = serviceTicket.MoneyCurrencyId;
            TypeId = serviceTicket.TypeId;
            TicketNumber = serviceTicket.TicketNumber;
            Description = serviceTicket.Description;
            Type = new SubtypeViewModel(serviceTicket.Type);
            MoneyCurrency = new CurrencyViewModel(serviceTicket.MoneyCurrency);
            Code = serviceTicket.Code;
            ProductsUsed = new ObservableCollection<MaterialYRepuestoViewModel>();
            if (serviceTicket.ProductsUsed != null)
                foreach (MaterialYRepuesto material in serviceTicket.ProductsUsed)
                    ProductsUsed.Add(new MaterialYRepuestoViewModel(material));
            CreationDate = serviceTicket.CreationDate;
            Title = serviceTicket.Title;
            WorkDone = serviceTicket.WorkDone;
            Technicians = new TechnicianViewModel[5];
            if (serviceTicket.Technicians != null)
                for (int i = 0; i < serviceTicket.Technicians.Count; i++)
                    Technicians[i] = new TechnicianViewModel(serviceTicket.Technicians[i]);
            Started = serviceTicket.Started;
            Finished = serviceTicket.Finished;
            HadLunch = serviceTicket.HadLunch;
            FeedbackSubmitted = serviceTicket.FeedbackSubmitted;
            RequiresQuotation = serviceTicket.RequiresQuotation;
            Email = serviceTicket.Email;
            #region General
            RXGenModel = serviceTicket.RXGenModel;
            RXGenSerial = serviceTicket.RXGenSerial;
            RXGenCreationDate = serviceTicket.RXGenCreationDate;
            RXGenCleanArea = serviceTicket.RXGenCleanArea;
            RXGenComments = serviceTicket.RXGenComments;
            RXGenVisitNumber = serviceTicket.RXGenVisitNumber;
            RXGenHWState = serviceTicket.RXGenHWState;
            #endregion
            #region Manteinance
            RXMantCheckLabels = serviceTicket.RXMantCheckLabels;
            RXMantCheckInOutSystem = serviceTicket.RXMantCheckInOutSystem;
            RXMantCheckIRFences = serviceTicket.RXMantCheckIRFences;
            RXMantCheckControlElements = serviceTicket.RXMantCheckControlElements;
            RXMantCheckEngineControl = serviceTicket.RXMantCheckEngineControl;
            RXMantCheckConveyorBelt = serviceTicket.RXMantCheckConveyorBelt;
            RXMantCheckEngineTraction = serviceTicket.RXMantCheckEngineTraction;
            RXMantCheckRollers = serviceTicket.RXMantCheckRollers;
            RXMantCheckEmergencyStop = serviceTicket.RXMantCheckEmergencyStop;
            RXMantCheckInterlock = serviceTicket.RXMantCheckInterlock;
            RXMantCheckVoltMonitor = serviceTicket.RXMantCheckSecurityCircuit;
            RXMantCheckSecurityCircuit = serviceTicket.RXMantCheckConditioningSystem;
            RXMantCheckConditioningSystem = serviceTicket.RXMantCheckConditioningSystem;
            RXMantCheckOS = serviceTicket.RXMantCheckOS;
            RXMantCheckXRCone = serviceTicket.RXMantCheckXRCone;
            RXMantCheckLineAndDetectionModules = serviceTicket.RXMantCheckLineAndDetectionModules;
            RXMantCheckConfiguration = serviceTicket.RXMantCheckConfiguration;
            RXMantCheckKeyboard = serviceTicket.RXMantCheckKeyboard;
            RXMantCheckMonitorConfiguration = serviceTicket.RXMantCheckMonitorConfiguration;
            RXMantCheckTwoWayMode = serviceTicket.RXMantCheckTwoWayMode;
            RXMantCheckRadiationIndicators = serviceTicket.RXMantCheckRadiationIndicators;
            #endregion
            #region Voltage
            RXVoltCheckHaveUPS = serviceTicket.RXVoltCheckHaveUPS;
            RXVoltCheckIsolationTransformator = serviceTicket.RXVoltCheckIsolationTransformator;
            RXMantCheckScreenType = serviceTicket.RXMantCheckScreenType;
            RXMantLeadState = serviceTicket.RXMantLeadState;
            RXVoltInVoltage = serviceTicket.RXVoltInVoltage;
            RXVoltGroundVoltage = serviceTicket.RXVoltGroundVoltage;
            RXVoltUPSCapacity = serviceTicket.RXVoltUPSCapacity;
            RXVoltUPSInVoltage = serviceTicket.RXVoltUPSInVoltage;
            RXVoltUPSGroundVoltage = serviceTicket.RXVoltUPSGroundVoltage;
            RXVoltGenerator1XROnVoltage = serviceTicket.RXVoltGenerator1XROnVoltage;
            RXVoltGenerator1XROffVoltage = serviceTicket.RXVoltGenerator1XROffVoltage;
            RXVoltGenerator1XROnAnodeVoltage = serviceTicket.RXVoltGenerator1XROnAnodeVoltage;
            RXVoltGenerator1XROnHighVoltage = serviceTicket.RXVoltGenerator1XROnHighVoltage;
            RXVoltGenerator2XROnVoltage = serviceTicket.RXVoltGenerator2XROnVoltage;
            RXVoltGenerator2XROffVoltage = serviceTicket.RXVoltGenerator2XROffVoltage;
            RXVoltGenerator2XROnAnodeVoltage = serviceTicket.RXVoltGenerator2XROnAnodeVoltage;
            RXVoltGenerator2XROnHighVoltage = serviceTicket.RXVoltGenerator2XROnHighVoltage;
            #endregion
            #region RadiationMetrics
            RXRadHWTrademark = serviceTicket.RXRadHWTrademark;
            RXRadHWModel = serviceTicket.RXRadHWModel;
            RXRadHWCalibrationDate = serviceTicket.RXRadHWCalibrationDate;
            RXRadHWCalibrationDueDate = serviceTicket.RXRadHWCalibrationDueDate;
            RXRadTunnelRadIn = serviceTicket.RXRadTunnelRadIn;
            RXRadTunnelRadOut = serviceTicket.RXRadTunnelRadOut;
            RXRadOperatorRad = serviceTicket.RXRadOperatorRad;
            RXRadRadiationState = serviceTicket.RXRadRadiationState;
            #endregion
            #region Software (Optional)
            RXSoftPhysicalDongleSerial = serviceTicket.RXSoftPhysicalDongleSerial;
            RXSoftSoftwareDongleSerial = serviceTicket.RXSoftSoftwareDongleSerial;
            RXSoftSoftwareVersion = serviceTicket.RXSoftSoftwareVersion;
            RXSoftHaveSEN = serviceTicket.RXSoftHaveSEN;
            RXSoftHaveHITIP = serviceTicket.RXSoftHaveHITIP;
            RXSoftHaveXPLORE = serviceTicket.RXSoftHaveXPLORE;
            RXSoftHaveHISPOT = serviceTicket.RXSoftHaveHISPOT;
            RXSoftHaveIMS = serviceTicket.RXSoftHaveIMS;
            RXSoftHaveXACT = serviceTicket.RXSoftHaveXACT;
            RXSoftHaveHDA = serviceTicket.RXSoftHaveHDA;
            RXSoftHaveXPORT = serviceTicket.RXSoftHaveXPORT;
            RXSoftHaveXTRAIN = serviceTicket.RXSoftHaveXTRAIN;
            RXSoftTechnology = serviceTicket.RXSoftTechnology;
            #endregion
            #region Calibration
            RXCalType1 = serviceTicket.RXCalType1;
            RXCalType2 = serviceTicket.RXCalType2;
            RXCalType3 = serviceTicket.RXCalType3;
            RXCalType4 = serviceTicket.RXCalType4;
            RXCalSteelPenetration = serviceTicket.RXCalSteelPenetration;
            RXCalWireResolution = serviceTicket.RXCalWireResolution;
            RXCalCalibrationState = serviceTicket.RXCalCalibrationState;
            #endregion
        }

        public ServiceTicket ToModel()
        {
            List<MaterialYRepuesto> modelmaterials = new List<MaterialYRepuesto>();
            List<Technician> modeltechs = new List<Technician>();
            foreach (MaterialYRepuestoViewModel material in ProductsUsed)
                modelmaterials.Add(material.ToModel());
            for (int i = 0; i < 5; i++)
                if (Technicians[i] != null)
                    modeltechs.Add(Technicians[i].ToModel());
            return new ServiceTicket
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                IncidentId = IncidentId,
                Code = Code,              
                CreationDate = CreationDate,
                Description = Description,
                Email = Email,
                FeedbackSubmitted = FeedbackSubmitted,
                Finished = Finished,
                HadLunch = HadLunch,
                MoneyCurrency = MoneyCurrency?.ToModel(),
                MoneyCurrencyId = MoneyCurrency?.SQLiteRecordId?? 0,
                ProductsUsed = modelmaterials,
                RequiresQuotation = RequiresQuotation,
                Started = Started,
                Technicians = modeltechs,
                TicketNumber = TicketNumber,
                Title = Title,
                Type = Type?.ToModel(),
                TypeId = Type?.SQLiteRecordId?? 0,
                WorkDone = WorkDone,
                RXGenCleanArea = RXGenCleanArea,
                RXGenComments = RXGenComments,
                RXGenCreationDate = RXGenCreationDate,
                RXGenHWState = RXGenHWState,
                RXGenModel = RXGenModel,
                RXGenSerial = RXGenSerial,
                RXGenVisitNumber = RXGenVisitNumber,
                RXMantCheckConditioningSystem = RXMantCheckConditioningSystem,
                RXMantCheckConfiguration = RXMantCheckConfiguration,
                RXMantCheckControlElements = RXMantCheckControlElements,
                RXMantCheckConveyorBelt = RXMantCheckConveyorBelt,
                RXMantCheckEmergencyStop = RXMantCheckEmergencyStop,
                RXMantCheckEngineControl = RXMantCheckEngineControl,
                RXMantCheckEngineTraction = RXMantCheckEngineTraction,
                RXMantCheckInOutSystem = RXMantCheckInOutSystem,
                RXMantCheckInterlock = RXMantCheckInterlock,
                RXMantCheckIRFences = RXMantCheckIRFences,
                RXMantCheckKeyboard = RXMantCheckKeyboard,
                RXMantCheckLabels = RXMantCheckLabels,
                RXMantCheckLineAndDetectionModules = RXMantCheckLineAndDetectionModules,
                RXMantCheckMonitorConfiguration = RXMantCheckMonitorConfiguration,
                RXMantCheckOS = RXMantCheckOS,
                RXMantCheckRadiationIndicators = RXMantCheckRadiationIndicators,
                RXMantCheckRollers = RXMantCheckRollers,
                RXMantCheckScreenType = RXMantCheckScreenType,
                RXMantCheckSecurityCircuit = RXMantCheckSecurityCircuit,
                RXMantCheckTwoWayMode = RXMantCheckTwoWayMode,
                RXMantCheckVoltMonitor = RXMantCheckVoltMonitor,
                RXMantCheckXRCone = RXMantCheckXRCone,
                RXMantLeadState = RXMantLeadState,
                RXVoltCheckHaveUPS = RXVoltCheckHaveUPS,
                RXVoltCheckIsolationTransformator = RXVoltCheckIsolationTransformator,
                RXVoltGenerator1XROffVoltage = RXVoltGenerator1XROffVoltage,
                RXVoltGenerator1XROnAnodeVoltage = RXVoltGenerator1XROnAnodeVoltage,
                RXVoltGenerator1XROnHighVoltage = RXVoltGenerator1XROnHighVoltage,
                RXVoltGenerator1XROnVoltage = RXVoltGenerator1XROnVoltage,
                RXVoltGenerator2XROffVoltage = RXVoltGenerator2XROffVoltage,
                RXVoltGenerator2XROnAnodeVoltage = RXVoltGenerator2XROnAnodeVoltage,
                RXVoltGenerator2XROnHighVoltage = RXVoltGenerator2XROnHighVoltage,
                RXVoltGenerator2XROnVoltage = RXVoltGenerator2XROnVoltage,
                RXVoltGroundVoltage = RXVoltGroundVoltage,
                RXVoltInVoltage = RXVoltInVoltage,
                RXVoltUPSCapacity = RXVoltUPSCapacity,
                RXVoltUPSGroundVoltage = RXVoltUPSGroundVoltage,
                RXVoltUPSInVoltage = RXVoltUPSInVoltage,
                RXRadHWCalibrationDate = RXRadHWCalibrationDate,
                RXRadHWCalibrationDueDate = RXRadHWCalibrationDueDate,
                RXRadHWModel = RXRadHWModel,
                RXRadHWTrademark = RXRadHWTrademark,
                RXRadOperatorRad = RXRadOperatorRad,
                RXRadRadiationState = RXRadRadiationState,
                RXRadTunnelRadIn = RXRadTunnelRadIn,
                RXRadTunnelRadOut = RXRadTunnelRadOut,
                RXCalCalibrationState = RXCalCalibrationState,
                RXCalSteelPenetration = RXCalSteelPenetration,
                RXCalType1 = RXCalType1,
                RXCalType2 = RXCalType2,
                RXCalType3 = RXCalType3,
                RXCalType4 = RXCalType4,
                RXCalWireResolution = RXCalWireResolution,
                RXSoftHaveHDA = RXSoftHaveHDA,
                RXSoftHaveHISPOT = RXSoftHaveHISPOT,
                RXSoftHaveHITIP = RXSoftHaveHITIP,
                RXSoftHaveIMS = RXSoftHaveIMS,
                RXSoftHaveSEN = RXSoftHaveSEN,
                RXSoftHaveXACT = RXSoftHaveXACT,
                RXSoftHaveXPLORE = RXSoftHaveXPLORE,
                RXSoftHaveXPORT = RXSoftHaveXPORT,
                RXSoftHaveXTRAIN = RXSoftHaveXTRAIN,
                RXSoftPhysicalDongleSerial = RXSoftPhysicalDongleSerial,
                RXSoftSoftwareDongleSerial = RXSoftSoftwareDongleSerial,
                RXSoftSoftwareVersion = RXSoftSoftwareVersion,
                RXSoftTechnology = RXSoftTechnology
            };
        }
        #endregion

        #region Methods
        public bool IsLegalizable() => ProductsUsed.Where(mat => mat.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Facturar).Count() > 0;
        #endregion
    }
}
