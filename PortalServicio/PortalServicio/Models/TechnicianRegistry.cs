using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class TechnicianRegistry
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Technician Technician { get; set; }
        [ForeignKey(typeof(Technician))]
        public int TechnicianId { get; set; }
        [ForeignKey(typeof(CDTTicket))]
        public int CDTTicketId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public double HoursNormal { get; set; }
        public double HoursNormalNight { get; set; }
        public double HoursDaytimeExtra { get; set; }
        public double HoursNightExtra { get; set; }
        public double HoursHolydayDaytime { get; set; }
        public double HoursHolydayNight { get; set; }
        public double HoursOffdayDaytime { get; set; }
        public double HoursOffdayNight { get; set; }
        public double HoursOffdayDaytimeExtra { get; set; }
        public double HoursOffdayNightExtra { get; set; }
        public bool IsDatetimeSet { get; set; }
    }
}
