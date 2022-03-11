using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class Legalization
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [MaxLength(20),NotNull]
        public string LegalizationNumber { get; set; }
        [Unique, NotNull]
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Currency MoneyCurrency { get; set; }
        [ForeignKey(typeof(Currency))]
        public int MoneyCurrencyId { get; set; }
        public Types.SPCLEGALIZATION_TYPE LegalizationType { get; set; }
        public Types.SPCLEGALIZATION_SIGNSTATE SignState { get; set; }
        public bool ProjectIssue { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Company Company { get; set; }
        [ForeignKey(typeof(Company))]
        public int CompanyId { get; set; }
        [MaxLength(20)]
        public string LastCreditCardDigits { get; set; }
        [MaxLength(1000)]
        public string Detail { get; set; }
        public decimal MoneyRequested { get; set; }
        public decimal MoneyPaid { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<LegalizationItem> LegalizationItems { get; set; }
        //[ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        //public ServiceTicket RelatedServiceTicket { get; set; }
        //[ForeignKey(typeof(ServiceTicket))]
        //public int RelatedServiceTicketId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Incident RelatedIncident { get; set; }
        [ForeignKey(typeof(Incident))]
        public int RelatedIncidentId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public CDT RelatedCDT { get; set; }
        [ForeignKey(typeof(CDT))]
        public int RelatedCDTId { get; set; }

    }
}
