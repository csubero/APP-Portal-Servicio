using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class NoteViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private int _ServiceTicketId;
        private ServiceTicket _ServiceTicket;
        private Incident _Incident;
        private CDTTicket _Ticket;
        private int _IncidentId;
        private int _TicketId;
        private Guid _InternalId;
        private Guid _ObjectId;
        private string _Filename;
        private string _Content;
        private string _Title;
        private string _Mime;

        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        public int ServiceTicketId
        {
            get { return _ServiceTicketId; }
            set { SetValue(ref _ServiceTicketId, value); }
        }
        public ServiceTicket ServiceTicket
        {
            get { return _ServiceTicket; }
            set { SetValue(ref _ServiceTicket, value); }
        }
        public Incident Incident
        {
            get { return _Incident; }
            set { SetValue(ref _Incident, value); }
        }
        public int IncidentId
        {
            get { return _IncidentId; }
            set { SetValue(ref _IncidentId, value); }
        }
        public int TicketId
        {
            get { return _TicketId; }
            set { SetValue(ref _TicketId, value); }
        }
        public CDTTicket Ticket
        {
            get { return _Ticket; }
            set { SetValue(ref _Ticket, value); }
        }
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        public Guid ObjectId
        {
            get { return _ObjectId; }
            set { SetValue(ref _ObjectId, value); }
        }
        public string Filename
        {
            get { return _Filename; }
            set { SetValue(ref _Filename, value); }
        }
        public string Content
        {
            get { return _Content; }
            set { SetValue(ref _Content, value); }
        }
        public string Title
        {
            get { return _Title; }
            set { SetValue(ref _Title, value); }
        }
        public string Mime
        {
            get { return _Mime; }
            set { SetValue(ref _Mime, value); }
        }
        #endregion

        #region Constructor
        public NoteViewModel(Note note)
        {
            if (note == null)
                return;
            InternalId = note.InternalId;
            SQLiteRecordId = note.SQLiteRecordId;
            Incident = note.Incident;
            IncidentId = note.IncidentId;
            ServiceTicket = note.ServiceTicket;
            ServiceTicketId = note.ServiceTicketId;
            Mime = note.Mime;
            ObjectId = note.ObjectId;
            Filename = note.Filename;
            Content = note.Content;
            Title = note.Title;
        }
        #endregion

        #region Converter
        public Note ToModel() =>
            new Note
            {
                InternalId = InternalId,
                Incident = Incident,
                IncidentId = IncidentId,
                ServiceTicket = ServiceTicket,
                ServiceTicketId = ServiceTicketId,
                SQLiteRecordId = SQLiteRecordId,
                ObjectId = ObjectId,
                Filename = Filename,
                Content = Content,
                Title = Title,
                Mime = Mime
            };
        #endregion
    }
}
