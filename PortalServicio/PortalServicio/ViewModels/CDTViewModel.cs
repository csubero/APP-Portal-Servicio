using PortalAPI.Contracts;
using PortalServicio.DTO;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PortalServicio.ViewModels
{
    public class CDTViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private DateTime _CreatedOn;
        private string _Number;
        private ClientViewModel _Client;
        private int _ClientId;
        private bool _IsFinalClient;
        private string _MonitoringAccountNumber;
        private string _MonitoringAccountName;
        private string _MainContact;
        private string _MainContactEmail;
        private string _MainContactPhone;
        private string _SecondaryContact;
        private string _SecondaryContactEmail;
        private string _SecondaryContactPhone;
        private string _Description;
        private SubtypeViewModel _System;
        private int _SystemId;
        private Types.SPCCDT_PROJECTSTATE _ProjectState;
        private DateTime _ProjectStartDate;
        private DateTime _ProjectClientDeadline;
        private bool _IsApprovedAdministration;
        private bool _IsApprovedComercial;
        private bool _IsApprovedFinancial;
        private bool _IsApprovedInstallation;
        private bool _IsApprovedOperations;
        private bool _IsApprovedPlanning;
        private bool _IsApprovedCustomerService;
        private SystemUserViewModel _ApproverAdministration;
        private int _ApproverAdministrationId;
        private SystemUserViewModel _ApproverComercial;
        private int _ApproverComercialId;
        private SystemUserViewModel _ApproverFinancial;
        private int _ApproverFinancialId;
        private SystemUserViewModel _ApproverInstallation;
        private int _ApproverInstallationId;
        private SystemUserViewModel _ApproverOperations;
        private int _ApproverOperationsId;
        private SystemUserViewModel _ApproverPlanning;
        private int _ApproverPlanningId;
        private SystemUserViewModel _ApproverCustomerService;
        private int _ApproverCustomerServiceId;
        private bool _IsApproved;
        private ObservableCollection<ProjectEquipmentViewModel> _ProjectEquipment;
        private ObservableCollection<ProjectMaterialViewModel> _ProjectMaterials;
        private ObservableCollection<EquipmentRequestOrderViewModel> _EquipmentRequestedOrders;
        private ObservableCollection<MaterialRequestOrderViewModel> _MaterialRequestedOrders;
        private ObservableCollection<CDTTicketViewModel> _CDTTickets;
        private ObservableCollection<ExtraEquipmentRequestViewModel> _ExtraEquipment;

        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        public string Number
        {
            get { return _Number; }
            set { SetValue(ref _Number, value); }
        }
        public ClientViewModel Client
        {
            get { return _Client; }
            set { SetValue(ref _Client, value); }
        }
        public int ClientId
        {
            get { return _ClientId; }
            set { SetValue(ref _ClientId, value); }
        }
        public bool IsFinalClient
        {
            get { return _IsFinalClient; }
            set { SetValue(ref _IsFinalClient, value); }
        }
        public string MonitoringAccountNumber
        {
            get { return _MonitoringAccountNumber; }
            set { SetValue(ref _MonitoringAccountNumber, value); }
        }
        public string MonitoringAccountName
        {
            get { return _MonitoringAccountName; }
            set { SetValue(ref _MonitoringAccountName, value); }
        }
        public string MainContact {
            get { return _MainContact; }
            set { SetValue(ref _MainContact, value); }
        }
        public string MainContactEmail
        {
            get { return _MainContactEmail; }
            set { SetValue(ref _MainContactEmail, value); }
        }
        public string MainContactPhone {
            get { return _MainContactPhone; }
            set { SetValue(ref _MainContactPhone, value); }
        }
        public string SecondaryContact {
            get { return _SecondaryContact; }
            set { SetValue(ref _SecondaryContact, value); }
        }
        public string SecondaryContactEmail {
            get { return _SecondaryContactEmail; }
            set { SetValue(ref _SecondaryContactEmail, value); }
        }
        public string SecondaryContactPhone {
            get { return _SecondaryContactPhone; }
            set { SetValue(ref _SecondaryContactPhone, value); }
        }
        public string Description
        {
            get { return _Description; }
            set { SetValue(ref _Description, value); }
        }
        public SubtypeViewModel System
        {
            get { return _System; }
            set { SetValue(ref _System, value); }
        }
        public int SystemId
        {
            get { return _SystemId; }
            set { SetValue(ref _SystemId, value); }
        }
        public Types.SPCCDT_PROJECTSTATE ProjectState
        {
            get { return _ProjectState; }
            set { SetValue(ref _ProjectState, value); }
        }
        public DateTime ProjectStartDate {
            get { return _ProjectStartDate; }
            set { SetValue(ref _ProjectStartDate, value); }
        }
        public DateTime ProjectClientDeadline {
            get { return _ProjectClientDeadline; }
            set { SetValue(ref _ProjectClientDeadline, value); }
        }
        public bool IsApprovedAdministration {
            get { return _IsApprovedAdministration; }
            set { SetValue(ref _IsApprovedAdministration, value); }
        }
        public bool IsApprovedComercial {
            get { return _IsApprovedComercial; }
            set { SetValue(ref _IsApprovedComercial, value); }
        }
        public bool IsApprovedFinancial
        {
            get { return _IsApprovedFinancial; }
            set { SetValue(ref _IsApprovedFinancial, value); }
        }
        public bool IsApprovedInstallation {
            get { return _IsApprovedInstallation; }
            set { SetValue(ref _IsApprovedInstallation, value); }
        }
        public bool IsApprovedOperations
        {
            get { return _IsApprovedOperations; }
            set { SetValue(ref _IsApprovedOperations, value); }
        }
        public bool IsApprovedPlanning {
            get { return _IsApprovedPlanning; }
            set { SetValue(ref _IsApprovedPlanning, value); }
        }
        public bool IsApprovedCustomerService {
            get { return _IsApprovedCustomerService; }
            set { SetValue(ref _IsApprovedCustomerService, value); }
        }
        public SystemUserViewModel ApproverAdministration {
            get { return _ApproverAdministration; }
            set { SetValue(ref _ApproverAdministration, value); }
        }
        public int ApproverAdministrationId {
            get { return _ApproverAdministrationId; }
            set { SetValue(ref _ApproverAdministrationId, value); }
        }
        public SystemUserViewModel ApproverComercial {
            get { return _ApproverComercial; }
            set { SetValue(ref _ApproverComercial, value); }
        }
        public int ApproverComercialId
        {
            get { return _ApproverComercialId; }
            set { SetValue(ref _ApproverComercialId, value); }
        }
        public SystemUserViewModel ApproverFinancial {
            get { return _ApproverFinancial; }
            set { SetValue(ref _ApproverFinancial, value); }
        }
        public int ApproverFinancialId {
            get { return _ApproverFinancialId; }
            set { SetValue(ref _ApproverFinancialId, value); }
        }
        public SystemUserViewModel ApproverInstallation {
            get { return _ApproverInstallation; }
            set { SetValue(ref _ApproverInstallation, value); }
        }
        public int ApproverInstallationId {
            get { return _ApproverInstallationId; }
            set { SetValue(ref _ApproverInstallationId, value); }
        }
        public SystemUserViewModel ApproverOperations {
            get { return _ApproverOperations; }
            set { SetValue(ref _ApproverOperations, value); }
        }
        public int ApproverOperationsId {
            get { return _ApproverOperationsId; }
            set { SetValue(ref _ApproverOperationsId, value); }
        }
        public SystemUserViewModel ApproverPlanning {
            get { return _ApproverPlanning; }
            set { SetValue(ref _ApproverPlanning, value); }
        }
        public int ApproverPlanningId {
            get { return _ApproverPlanningId; }
            set { SetValue(ref _ApproverPlanningId, value); }
        }
        public SystemUserViewModel ApproverCustomerService
        {
            get { return _ApproverCustomerService; }
            set { SetValue(ref _ApproverCustomerService, value); }
        }
        public int ApproverCustomerServiceId
        {
            get { return _ApproverCustomerServiceId; }
            set { SetValue(ref _ApproverCustomerServiceId, value); }
        }
        public bool IsApproved {
            get { return _IsApproved; }
            set { SetValue(ref _IsApproved, value); }
        }
        public ObservableCollection<ProjectEquipmentViewModel> ProjectEquipment
        {
            get { return _ProjectEquipment; }
            set { SetValue(ref _ProjectEquipment, value); }
        }
        public ObservableCollection<ProjectMaterialViewModel> ProjectMaterials
        {
            get { return _ProjectMaterials; }
            set { SetValue(ref _ProjectMaterials, value); }
        }
        public ObservableCollection<EquipmentRequestOrderViewModel> EquipmentRequestedOrders
        {
            get { return _EquipmentRequestedOrders; }
            set { SetValue(ref _EquipmentRequestedOrders, value); }
        }
        public ObservableCollection<MaterialRequestOrderViewModel> MaterialRequestedOrders
        {
            get { return _MaterialRequestedOrders; }
            set { SetValue(ref _MaterialRequestedOrders, value); }
        }
        public ObservableCollection<CDTTicketViewModel> CDTTickets
        {
            get { return _CDTTickets; }
            set { SetValue(ref _CDTTickets, value); }
        }
        public ObservableCollection<ExtraEquipmentRequestViewModel> ExtraEquipment
        {
            get { return _ExtraEquipment; }
            set { SetValue(ref _ExtraEquipment, value); }
        }
        public DateTime CreatedOn { get { return _CreatedOn; } set { SetValue(ref _CreatedOn, value); } }
        #endregion

        public CDTViewModel(CDT cdt)
        {
            if (cdt == null)
                return;
            InternalId = cdt.InternalId;
            CreatedOn = cdt.CreatedOn;
            SQLiteRecordId = cdt.SQLiteRecordId;
            Client = new ClientViewModel(cdt.Client);
            ClientId = cdt.ClientId;
            System = new SubtypeViewModel(cdt.System);
            SystemId = cdt.SystemId;
            ProjectEquipment = new ObservableCollection<ProjectEquipmentViewModel>();
            if (cdt.ProjectEquipment != null)
                foreach (ProjectEquipment equipment in cdt.ProjectEquipment)
                    ProjectEquipment.Add(new ProjectEquipmentViewModel(equipment));
            ProjectMaterials = new ObservableCollection<ProjectMaterialViewModel>();
            if(cdt.ProjectMaterials!=null)
                foreach (ProjectMaterial material in cdt.ProjectMaterials)
                    ProjectMaterials.Add(new ProjectMaterialViewModel(material));
            EquipmentRequestedOrders = new ObservableCollection<EquipmentRequestOrderViewModel>();
            if (cdt.EquipmentRequestedOrders != null)
                foreach (EquipmentRequestOrder request in cdt.EquipmentRequestedOrders)
                    EquipmentRequestedOrders.Add(new EquipmentRequestOrderViewModel(request));
            MaterialRequestedOrders = new ObservableCollection<MaterialRequestOrderViewModel>();
            if (cdt.MaterialRequestedOrders != null)
                foreach (MaterialRequestOrder request in cdt.MaterialRequestedOrders)
                    MaterialRequestedOrders.Add(new MaterialRequestOrderViewModel(request));
            CDTTickets = new ObservableCollection<CDTTicketViewModel>();
            if (cdt.CDTTickets != null)
                foreach (CDTTicket ticket in cdt.CDTTickets)
                    CDTTickets.Add(new CDTTicketViewModel(ticket));
            ExtraEquipment = new ObservableCollection<ExtraEquipmentRequestViewModel>();
            if (cdt.ExtraEquipment != null)
                foreach (ExtraEquipmentRequest ExEquip in cdt.ExtraEquipment)
                    ExtraEquipment.Add(new ExtraEquipmentRequestViewModel(ExEquip));
            if (cdt.ApproverAdministration != null)
            {
                ApproverAdministration = new SystemUserViewModel(cdt.ApproverAdministration);
                ApproverAdministrationId = cdt.ApproverAdministrationId;
            }
            if (cdt.ApproverComercial!=null) {
                ApproverComercial = new SystemUserViewModel(cdt.ApproverComercial);
                ApproverComercialId = cdt.ApproverComercialId;
            }
            if (cdt.ApproverCustomerService != null)
            {
                ApproverCustomerService = new SystemUserViewModel(cdt.ApproverCustomerService);
                ApproverCustomerServiceId = cdt.ApproverCustomerServiceId;
            }
            if (cdt.ApproverFinancial!=null){
                ApproverFinancial = new SystemUserViewModel(cdt.ApproverFinancial);
                ApproverFinancialId = cdt.ApproverFinancialId;
            }
            if (cdt.ApproverInstallation!=null) {
                ApproverInstallation = new SystemUserViewModel(cdt.ApproverInstallation);
                ApproverInstallationId = cdt.ApproverInstallationId;
            }
            if (cdt.ApproverOperations != null)
            {
                ApproverOperations = new SystemUserViewModel(cdt.ApproverOperations);
                ApproverOperationsId = cdt.ApproverOperationsId;
            }
            if (cdt.ApproverPlanning != null)
            {
                ApproverPlanning = new SystemUserViewModel(cdt.ApproverPlanning);
                ApproverPlanningId = cdt.ApproverPlanningId;
            }
            Description = cdt.Description;
            IsApproved = cdt.IsApproved;
            IsApprovedAdministration = cdt.IsApprovedAdministration;
            IsApprovedComercial = cdt.IsApprovedComercial;
            IsApprovedCustomerService = cdt.IsApprovedCustomerService;
            IsApprovedFinancial = cdt.IsApprovedFinancial;
            IsApprovedInstallation = cdt.IsApprovedInstallation;
            IsApprovedOperations = cdt.IsApprovedOperations;
            IsApprovedPlanning = cdt.IsApprovedPlanning;
            IsFinalClient = cdt.IsFinalClient;
            MainContact = cdt.MainContact;
            MainContactEmail = cdt.MainContactEmail;
            MainContactPhone = cdt.MainContactPhone;
            MonitoringAccountName = cdt.MonitoringAccountName;
            MonitoringAccountNumber = cdt.MonitoringAccountNumber;
            Number = cdt.Number;
            ProjectClientDeadline = cdt.ProjectClientDeadline;
            ProjectStartDate = cdt.ProjectStartDate;
            ProjectState = cdt.ProjectState;
            SecondaryContact = cdt.SecondaryContact;
            SecondaryContactEmail = cdt.SecondaryContactEmail;
            SecondaryContactPhone = cdt.SecondaryContactPhone;
        }

        public CDTViewModel(DTO_CDTLookUp cdt)
        {
            if (cdt == null)
                return;
            InternalId = cdt.InternalId;
            Client = new ClientViewModel(cdt.Client);
            Number = cdt.Number;
            CreatedOn = cdt.CreatedOn;
        }

        public CDT ToModel()
        {
            #region Convert Equipment of CDT
            List<ProjectEquipment> equipment = new List<ProjectEquipment>();
            if (ProjectEquipment != null)
                foreach (ProjectEquipmentViewModel equip in ProjectEquipment)
                    equipment.Add(equip.ToModel());
            #endregion
            #region Convert Materials of CDT
            List<ProjectMaterial> materials = new List<ProjectMaterial>();
            if (ProjectMaterials != null)
                foreach (ProjectMaterialViewModel material in ProjectMaterials)
                    materials.Add(material.ToModel());
            #endregion
            #region Convert Equipment Requests
            List<EquipmentRequestOrder> equipmentRequests = new List<EquipmentRequestOrder>();
            if (EquipmentRequestedOrders != null)
                foreach (EquipmentRequestOrderViewModel request in EquipmentRequestedOrders)
                    equipmentRequests.Add(request.ToModel());
            #endregion
            #region Convert Material Requests
            List<MaterialRequestOrder> materialRequests = new List<MaterialRequestOrder>();
            if (MaterialRequestedOrders != null)
                foreach (MaterialRequestOrderViewModel request in MaterialRequestedOrders)
                    materialRequests.Add(request.ToModel());
            #endregion
            #region Convert CDTTickets
            List<CDTTicket> tickets = new List<CDTTicket>();
            if (CDTTickets != null)
                foreach (CDTTicketViewModel ticket in CDTTickets)
                    tickets.Add(ticket.ToModel());
            #endregion
            #region Convert Extra Equipment
            List<ExtraEquipmentRequest> exEquips = new List<ExtraEquipmentRequest>();
            if (ExtraEquipment != null)
                foreach (ExtraEquipmentRequestViewModel exEq in ExtraEquipment)
                    exEquips.Add(exEq.ToModel());
            #endregion
            return new CDT
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                CreatedOn = CreatedOn,
                Client = Client?.ToModel(),
                ClientId = ClientId,
                System = System?.ToModel(),
                SystemId = SystemId,
                ProjectEquipment = equipment,
                ProjectMaterials = materials,
                EquipmentRequestedOrders = equipmentRequests,
                MaterialRequestedOrders = materialRequests,
                CDTTickets = tickets,
                ExtraEquipment =  exEquips,
                ApproverAdministration = ApproverAdministration?.ToModel(),
                ApproverAdministrationId = ApproverAdministrationId,
                ApproverComercial = ApproverComercial?.ToModel(),
                ApproverComercialId = ApproverComercialId,
                ApproverCustomerService = ApproverCustomerService?.ToModel(),
                ApproverCustomerServiceId = ApproverCustomerServiceId,
                ApproverFinancial = ApproverFinancial?.ToModel(),
                ApproverFinancialId = ApproverFinancialId,
                ApproverInstallation =ApproverInstallation?.ToModel(),
                ApproverInstallationId = ApproverInstallationId,
                ApproverOperations = ApproverOperations?.ToModel(),
                ApproverOperationsId = ApproverOperationsId,
                ApproverPlanning = ApproverPlanning?.ToModel(),
                ApproverPlanningId = ApproverPlanningId,
                Description = Description,
                IsApproved = IsApproved,
                IsApprovedAdministration = IsApprovedAdministration,
                IsApprovedComercial = IsApprovedComercial,
                IsApprovedCustomerService = IsApprovedCustomerService,
                IsApprovedFinancial = IsApprovedFinancial,
                IsApprovedInstallation = IsApprovedInstallation,
                IsApprovedOperations = IsApprovedOperations,
                IsApprovedPlanning = IsApprovedPlanning,
                IsFinalClient = IsFinalClient,
                MainContact = MainContact,
                MainContactEmail = MainContactEmail,
                MainContactPhone = MainContactPhone,
                MonitoringAccountName = MonitoringAccountName,
                MonitoringAccountNumber = MonitoringAccountNumber,
                Number = Number,
                ProjectClientDeadline = ProjectClientDeadline,
                ProjectStartDate = ProjectStartDate,
                ProjectState = ProjectState,
                SecondaryContact = SecondaryContact,
                SecondaryContactEmail = SecondaryContactEmail,
                SecondaryContactPhone = SecondaryContactPhone,
            };
        }
    }
}
