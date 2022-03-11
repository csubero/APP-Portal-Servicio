using PortalAPI.Contracts;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PortalServicio.ViewModels
{
    public class LegalizationViewModel : BaseViewModel
    {
        public int _SQLiteRecordId;
        public string _LegalizationNumber;
        public Guid _InternalId;
        public CurrencyViewModel _MoneyCurrency;
        public int _MoneyCurrencyId;
        public Types.SPCLEGALIZATION_TYPE _LegalizationType;
        public Types.SPCLEGALIZATION_SIGNSTATE _SignState;
        public bool _ProjectIssue;
        public string _Detail;
        public CompanyViewModel _Company;
        public int _CompanyId;
        public string _LastCreditCardDigits;
        public decimal _MoneyRequested;
        public decimal _MoneyPaid;
        public bool _IsMoneyPending;
        public ObservableCollection<LegalizationItemViewModel> _LegalizationItems;
        //public ServiceTicketViewModel _RelatedServiceTicket;
        //public int _RelatedServiceTicketId;
        public IncidentViewModel _RelatedIncident;
        public int _RelatedIncidentId;
        public CDTViewModel _RelatedCDT;
        public int _RelatedCDTId;

        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        public bool IsMoneyPending
        {
            get { return _IsMoneyPending; }
            set { SetValue(ref _IsMoneyPending, value); }
        }
        public string LegalizationNumber
        {
            get { return _LegalizationNumber; }
            set { SetValue(ref _LegalizationNumber, value); }
        }
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        public CurrencyViewModel MoneyCurrency
        {
            get { return _MoneyCurrency; }
            set { SetValue(ref _MoneyCurrency, value); }
        }
        public int MoneyCurrencyId
        {
            get { return _MoneyCurrencyId; }
            set { SetValue(ref _MoneyCurrencyId, value); }
        }
        public int CompanyId
        {
            get { return _CompanyId; }
            set { SetValue(ref _CompanyId, value); }
        }
        public Types.SPCLEGALIZATION_TYPE LegalizationType
        {
            get { return _LegalizationType; }
            set { SetValue(ref _LegalizationType, value); }
        }
        public Types.SPCLEGALIZATION_SIGNSTATE SignState
        {
            get { return _SignState; }
            set { SetValue(ref _SignState, value); }
        }
        public bool ProjectIssue
        {
            get { return _ProjectIssue; }
            set { SetValue(ref _ProjectIssue, value); }
        }
        public CompanyViewModel Company
        {
            get { return _Company; }
            set { SetValue(ref _Company, value); }
        }
        public string Detail
        {
            get { return _Detail; }
            set { SetValue(ref _Detail, value); }
        }
        public string LastCreditCardDigits
        {
            get { return _LastCreditCardDigits; }
            set { SetValue(ref _LastCreditCardDigits, value); }
        }
        public decimal MoneyRequested
        {
            get { return _MoneyRequested; }
            set { SetValue(ref _MoneyRequested, value); }
        }
        public decimal MoneyPaid
        {
            get { return _MoneyPaid; }
            set { SetValue(ref _MoneyPaid, value); }
        }
        public decimal MoneyPending
        {
            get { return _MoneyRequested - _MoneyPaid; }
        }
        //public ServiceTicketViewModel RelatedServiceTicket
        //{
        //    get { return _RelatedServiceTicket; }
        //    set { SetValue(ref _RelatedServiceTicket, value); }
        //}
        //public int RelatedServiceTicketId
        //{
        //    get { return _RelatedServiceTicketId; }
        //    set { SetValue(ref _RelatedServiceTicketId, value); }
        //}
        public IncidentViewModel RelatedIncident
        {
            get { return _RelatedIncident; }
            set { SetValue(ref _RelatedIncident, value); }
        }
        public int RelatedIncidentId
        {
            get { return _RelatedIncidentId; }
            set { SetValue(ref _RelatedIncidentId, value); }
        }
        public CDTViewModel RelatedCDT
        {
            get { return _RelatedCDT; }
            set { SetValue(ref _RelatedCDT, value); }
        }
        public int RelatedCDTId
        {
            get { return _RelatedCDTId; }
            set { SetValue(ref _RelatedCDTId, value); }
        }
        public ObservableCollection<LegalizationItemViewModel> LegalizationItems
        {
            get { return _LegalizationItems; }
            set { SetValue(ref _LegalizationItems, value); }
        }

        public LegalizationViewModel(Legalization legalization)
        {
            if (legalization == null)
                return;
            InternalId = legalization.InternalId;
            SQLiteRecordId = legalization.SQLiteRecordId;
            LegalizationNumber = legalization.LegalizationNumber;
            MoneyCurrency = new CurrencyViewModel(legalization.MoneyCurrency);
            MoneyCurrencyId = legalization.MoneyCurrencyId;
            LegalizationType = legalization.LegalizationType;
            SignState = legalization.SignState;
            ProjectIssue = legalization.ProjectIssue;
            Company = legalization.Company != null ? new CompanyViewModel(legalization.Company) : null;
            CompanyId = legalization.CompanyId;
            Detail = legalization.Detail;
            MoneyRequested = legalization.MoneyRequested;
            MoneyPaid = legalization.MoneyPaid;
            IsMoneyPending = legalization.MoneyRequested - MoneyPaid > 0;
            LastCreditCardDigits = legalization.LastCreditCardDigits;
            //if (legalization.RelatedServiceTicket != null)
            //    RelatedServiceTicket = new ServiceTicketViewModel(legalization.RelatedServiceTicket);
            //RelatedServiceTicketId = legalization.RelatedServiceTicketId;
            if (legalization.RelatedIncident != null)
                RelatedIncident = new IncidentViewModel(legalization.RelatedIncident);
            RelatedIncidentId = legalization.RelatedIncidentId;
            if (legalization.RelatedCDT != null)
                RelatedCDT = new CDTViewModel(legalization.RelatedCDT);
            RelatedCDTId = legalization.RelatedCDTId;
            LegalizationItems = new ObservableCollection<LegalizationItemViewModel>();
            if (legalization.LegalizationItems != null)
                foreach (LegalizationItem item in legalization.LegalizationItems)
                    LegalizationItems.Add(new LegalizationItemViewModel(item));
        }

        public LegalizationViewModel(DTO.DTO_LegalizationLookUp legalization)
        {
            if (legalization == null)
                return;
            InternalId = legalization.InternalId;
            LegalizationNumber = legalization.LegalizationNumber;
            LegalizationType = legalization.LegalizationType;
            MoneyRequested = legalization.MoneyRequested;
            MoneyPaid = legalization.MoneyPaid;
            SignState = legalization.SignState;
            MoneyCurrency = new CurrencyViewModel(legalization.MoneyCurrency);
            MoneyCurrencyId = legalization?.MoneyCurrency?.SQLiteRecordId ?? 0;
        }

        public Legalization ToModel()
        {
            List<LegalizationItem> legalizationItems = new List<LegalizationItem>();
            foreach (LegalizationItemViewModel item in LegalizationItems)
                legalizationItems.Add(item.ToModel());
            return new Legalization
            {
                SQLiteRecordId = SQLiteRecordId,
                LegalizationNumber = LegalizationNumber,
                InternalId = InternalId,
                Detail = Detail,
                Company = Company?.ToModel(),
                CompanyId = CompanyId,
                LegalizationType = LegalizationType,
                SignState = SignState,
                MoneyCurrency = MoneyCurrency?.ToModel(),
                MoneyCurrencyId = MoneyCurrencyId,
                MoneyPaid = MoneyPaid,
                MoneyRequested = MoneyRequested,
                ProjectIssue = ProjectIssue,
                LastCreditCardDigits = LastCreditCardDigits,
                RelatedCDT = RelatedCDT?.ToModel(),
                RelatedCDTId = RelatedCDTId,
                RelatedIncident = RelatedIncident?.ToModel(),
                RelatedIncidentId = RelatedIncidentId,
                LegalizationItems = legalizationItems
                //RelatedServiceTicket = RelatedServiceTicket?.ToModel(),
                //RelatedServiceTicketId = RelatedServiceTicketId
            };
        }
    }
}
