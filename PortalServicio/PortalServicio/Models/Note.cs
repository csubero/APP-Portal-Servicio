using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class Note
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        public Guid ObjectId { get; set; }
        [MaxLength(50)]
        public string Filename { get; set; }
        [MaxLength(50),NotNull]
        public string Mime { get; set; }
        [MaxLength(50000)]
        public string Content { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        [ForeignKey(typeof(ServiceTicket))]
        public int ServiceTicketId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public ServiceTicket ServiceTicket { get; set; }
        [ForeignKey(typeof(Incident))]
        public int IncidentId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Incident Incident { get; set; }
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public CDT CDT { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public CDTTicket Ticket { get; set; }
        [ForeignKey(typeof(CDTTicket))]
        public int TicketId { get; set; }
        #endregion
    } 
}
