using PortalAPI.Contracts;
using PortalAPI.DTO;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PortalServicio.ViewModels
{
    public class IncidentViewModel : BaseViewModel
    {
        #region Properties
        private ClientViewModel _Client;
        public ClientViewModel Client { get { return _Client; } set { SetValue(ref _Client, value); } }
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private DateTime _CreatedOn;
        public DateTime CreatedOn { get { return _CreatedOn; } set { SetValue(ref _CreatedOn, value); } }
        private SubtypeViewModel _Type;
        public SubtypeViewModel Type { get { return _Type; } set { SetValue(ref _Type, value); } }
        private CurrencyViewModel _MoneyCurrency;
        public CurrencyViewModel MoneyCurrency { get { return _MoneyCurrency; } set { SetValue(ref _MoneyCurrency, value); } }
        private string _Incidence;
        public string Incidence { get { return _Incidence; } set { SetValue(ref _Incidence, value); } }
        private bool _IsRequiringAssesment;
        public bool IsRequiringAssesment { get { return _IsRequiringAssesment; } set { SetValue(ref _IsRequiringAssesment, value); } }
        private bool _CreatedToday;
        public bool CreatedToday { get { return _CreatedToday; } set { SetValue(ref _CreatedToday, value); } }
        private bool _IsLocallySaved;
        public bool IsLocallySaved { get { return _IsLocallySaved; } set { SetValue(ref _IsLocallySaved, value); } }
        private string _Title;
        public string Title { get { return _Title; } set { SetValue(ref _Title, value); } }
        private string _Description;
        public string Description { get { return _Description; } set { SetValue(ref _Description, value); } }
        private string _Representative;
        public string Representative { get { return _Representative; } set { SetValue(ref _Representative, value); } }
        private string _TicketNumber;
        public string TicketNumber { get { return _TicketNumber; } set { SetValue(ref _TicketNumber, value); } }
        private string _ClientFeedback;
        public string ClientFeedback { get { return _ClientFeedback; } set { SetValue(ref _ClientFeedback, value); } }
        private double _FeedbackAnswer1;
        public double FeedbackAnswer1 { get { return _FeedbackAnswer1; } set { SetValue(ref _FeedbackAnswer1, value); } }
        private double _FeedbackAnswer2;
        public double FeedbackAnswer2 { get { return _FeedbackAnswer2; } set { SetValue(ref _FeedbackAnswer2, value); } }
        private ObservableCollection<TechnicianViewModel> _TechniciansAssigned;
        public ObservableCollection<TechnicianViewModel> TechniciansAssigned { get { return _TechniciansAssigned; } set { SetValue(ref _TechniciansAssigned, value); } }
        private DateTime _ProgrammedDate;
        public DateTime ProgrammedDate { get { return _ProgrammedDate; } set { SetValue(ref _ProgrammedDate, value); } }
        private ObservableCollection<ServiceTicketViewModel> _ServiceTickets;
        public ObservableCollection<ServiceTicketViewModel> ServiceTickets { get { return _ServiceTickets; } set { SetValue(ref _ServiceTickets, value); } }
        private Types.SPCINCIDENT_PAYMENTOPTION _PaymentOption;
        public Types.SPCINCIDENT_PAYMENTOPTION PaymentOption { get { return _PaymentOption; } set { SetValue(ref _PaymentOption, value); } }
        private Types.SPCINCIDENT_CONTROLOPTION _ControlOption;
        public Types.SPCINCIDENT_CONTROLOPTION ControlOption { get { return _ControlOption; } set { SetValue(ref _ControlOption, value); } }
        private bool _Reviewed;
        public bool Reviewed { get { return _Reviewed; } set { SetValue(ref _Reviewed, value); } }
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private int _TypeId;
        public int TypeId { get { return _TypeId; } set { SetValue(ref _TypeId, value); } }
        private int _ClientId;
        public int ClientId { get { return _ClientId; } set { SetValue(ref _ClientId, value); } }
        private int _MoneyCurrencyId;
        public int MoneyCurrencyId { get { return _MoneyCurrencyId; } set { SetValue(ref _MoneyCurrencyId, value); } }
        #endregion

        public IncidentViewModel(Incident incident)
        {
            if (incident == null)
                return;
            InternalId = incident.InternalId;
            SQLiteRecordId = incident.SQLiteRecordId;
            Client = new ClientViewModel(incident.Client);
            ClientId = incident.ClientId;
            CreatedOn = incident.CreatedOn;
            CreatedToday = incident.CreatedOn.DayOfYear == DateTime.Now.DayOfYear && CreatedOn.Year == DateTime.Now.Year;
            Type = incident.Type!=null?new SubtypeViewModel(incident.Type):null;
            TypeId = incident.TypeId;
            MoneyCurrency = new CurrencyViewModel(incident.MoneyCurrency);
            MoneyCurrencyId = incident.MoneyCurrencyId;
            Incidence = incident.Incidence;
            IsRequiringAssesment = incident.IsRequiringAssesment;
            Title = incident.Title;
            Description = incident.Description;
            Representative = incident.Representative;
            TicketNumber = incident.TicketNumber;
            ProgrammedDate = incident.ProgrammedDate;
            PaymentOption = incident.PaymentOption;
            ControlOption = incident.ControlOption;
            Reviewed = incident.Reviewed;
            ClientFeedback = incident.ClientFeedback;
            FeedbackAnswer1 = incident.FeedbackAnswer1;
            FeedbackAnswer2 = incident.FeedbackAnswer2;
            TechniciansAssigned = new ObservableCollection<TechnicianViewModel>();
            if (incident.TechniciansAssigned != null)
                foreach (Technician technician in incident.TechniciansAssigned)
                    TechniciansAssigned.Add(new TechnicianViewModel(technician));
            ServiceTickets = new ObservableCollection<ServiceTicketViewModel>();
            if (incident.ServiceTickets != null)
                foreach (ServiceTicket serviceTicket in incident.ServiceTickets)
                    ServiceTickets.Add(new ServiceTicketViewModel(serviceTicket));
        }

        public IncidentViewModel(DTO_IncidentLookUp incident)
        {
            if (incident == null)
                return;
            InternalId = incident.InternalId;
            Client = new ClientViewModel(incident.Client);
            CreatedOn = incident.CreatedOn;
            CreatedToday = incident.CreatedOn.DayOfYear == DateTime.Now.DayOfYear && CreatedOn.Year == DateTime.Now.Year;
            TicketNumber = incident.TicketNumber;
            ControlOption = incident.Control;
        }

        public Incident ToModel()
        {
            List<ServiceTicket> modeltickets = new List<ServiceTicket>();
            List<Technician> modeltechs = new List<Technician>();
            if (ServiceTickets != null)
                foreach (ServiceTicketViewModel ticket in ServiceTickets)
                    modeltickets.Add(ticket.ToModel());
            if (TechniciansAssigned != null)
                foreach (TechnicianViewModel tech in TechniciansAssigned)
                    modeltechs.Add(tech.ToModel());
            return new Incident
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Client = Client?.ToModel(),
                ClientId = ClientId,
                ControlOption = ControlOption,
                CreatedOn = CreatedOn,
                Description = Description,
                Incidence = Incidence,
                IsRequiringAssesment = IsRequiringAssesment,
                MoneyCurrency = MoneyCurrency?.ToModel(),
                MoneyCurrencyId = MoneyCurrency.SQLiteRecordId,
                PaymentOption = PaymentOption,
                ProgrammedDate = ProgrammedDate,
                Representative = Representative,
                Reviewed = Reviewed,
                ClientFeedback = ClientFeedback,
                FeedbackAnswer1 = FeedbackAnswer1,
                FeedbackAnswer2 = FeedbackAnswer2,
                ServiceTickets = modeltickets,
                TechniciansAssigned = modeltechs,
                TicketNumber = TicketNumber,
                Title = Title,
                Type = Type?.ToModel(),
                TypeId = Type!=null?Type.SQLiteRecordId:0
            };
        }
    }
}
