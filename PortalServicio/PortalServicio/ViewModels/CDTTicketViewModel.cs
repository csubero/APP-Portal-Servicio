using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PortalServicio.ViewModels
{
    public class CDTTicketViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private int _CDTId;
        private Guid _InternalId;
        private string _Number;
        private string _Workdone;
        private string _Agreements;
        private string _Email;
        private bool _HadLunch;
        private DateTime _Started;
        private DateTime _Finished;
        private ObservableCollection<TechnicianRegistryViewModel> _TechniciansRegistered;
        private bool _IsOpen;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public int CDTId { get { return _CDTId; } set { SetValue(ref _CDTId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } } 
        public string Number { get { return _Number; } set { SetValue(ref _Number, value); } }
        public string Workdone { get { return _Workdone; } set { SetValue(ref _Workdone, value); } }
        public string Agreements { get { return _Agreements; } set { SetValue(ref _Agreements, value); } }
        public string Email { get { return _Email; } set { SetValue(ref _Email, value); } }
        public bool HadLunch { get { return _HadLunch; } set { SetValue(ref _HadLunch, value); } }
        public DateTime Started { get { return _Started; } set { SetValue(ref _Started, value); } }
        public DateTime Finished { get { return _Finished; } set { SetValue(ref _Finished, value); IsOpen = value == default(DateTime); } }
        public ObservableCollection<TechnicianRegistryViewModel> TechniciansRegistered { get { return _TechniciansRegistered; } set { SetValue(ref _TechniciansRegistered, value); } }
        public bool IsOpen { get { return _IsOpen; } set { SetValue(ref _IsOpen, value); } }
        #endregion

        #region Constructors
        public CDTTicketViewModel(CDTTicket cdtTicket)
        {
            if (cdtTicket == null)
                return;
            InternalId = cdtTicket.InternalId;
            SQLiteRecordId = cdtTicket.SQLiteRecordId;
            CDTId = cdtTicket.CDTId;
            Started = cdtTicket.Started;
            Finished = cdtTicket.Finished;
            Workdone = cdtTicket.Workdone;
            Email = cdtTicket.Email;
            Agreements = cdtTicket.Agreements;
            HadLunch = cdtTicket.HadLunch;
            Number = cdtTicket.Number;
            TechniciansRegistered = new ObservableCollection<TechnicianRegistryViewModel>();
            if (cdtTicket.TechniciansRegistered != null)
                foreach (TechnicianRegistry tr in cdtTicket.TechniciansRegistered)
                    TechniciansRegistered.Add(new TechnicianRegistryViewModel(tr));
        }

        public CDTTicket ToModel()
        {
            List<TechnicianRegistry> trs = new List<TechnicianRegistry>();
            foreach (TechnicianRegistryViewModel tr in TechniciansRegistered)
                trs.Add(tr.ToModel());
            return new CDTTicket
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Agreements = Agreements,
                Number = Number,
                HadLunch = HadLunch,
                Workdone = Workdone,
                Email = Email,
                CDTId = CDTId,
                Finished = Finished,
                Started = Started,
                TechniciansRegistered = trs
            };
        }
        #endregion
    }
}
