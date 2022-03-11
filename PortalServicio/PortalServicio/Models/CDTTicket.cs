using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class CDTTicket
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        [MaxLength(25), NotNull]
        public string Number { get; set; }
        [MaxLength(1000)]
        public string Workdone { get; set; }
        [MaxLength(1000)]
        public string Agreements { get; set; }
        [MaxLength(30)]
        public string Email { get; set; }
        public bool HadLunch { get; set; }
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        public List<TechnicianRegistry> TechniciansRegistered { get; set; }
    }
}
