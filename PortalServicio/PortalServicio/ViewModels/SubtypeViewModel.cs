using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class SubtypeViewModel : BaseViewModel
    {
        #region Properties
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private string _Name;
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        //private List<ServiceTicket> _ServiceTickets;
        //public List<ServiceTicket> ServiceTickets { get { return _ServiceTickets; } set { SetValue(ref _ServiceTickets, value); } }
        //private List<Incident> _Incidents;
        //public List<Incident> Incidents { get { return _Incidents; } set { SetValue(ref _Incidents, value); } }
        #endregion

        public SubtypeViewModel(Subtype subtype)
        {
            if (subtype == null)
                return;
            InternalId = subtype.InternalId;
            SQLiteRecordId = subtype.SQLiteRecordId;
            Name = subtype.Name;
        }

        public Subtype ToModel() =>
            new Subtype
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name,
            };
    }
}
