using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class Incident
    {
        #region Properties
        [ForeignKey(typeof(Client))]
        public int ClientId { get; set; }
        [ForeignKey(typeof(Subtype))]
        public int TypeId { get; set; }
        [ForeignKey(typeof(Currency))]
        public int MoneyCurrencyId { get; set; }
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }

        [Unique]
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Client Client { get; set; }
        public DateTime CreatedOn { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Subtype Type { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Currency MoneyCurrency { get; set; }
        [MaxLength(25)]
        public string Incidence { get; set; }
        public bool IsRequiringAssesment { get; set; }
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [MaxLength(50)]
        public string Representative { get; set; }
        [MaxLength(50),NotNull]
        public string TicketNumber { get; set; }
        [ManyToMany(typeof(IncidentTechnician), CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<Technician> TechniciansAssigned { get; set; }
        public DateTime ProgrammedDate { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<ServiceTicket> ServiceTickets { get; set; }
        public Types.SPCINCIDENT_PAYMENTOPTION PaymentOption { get; set; }
        public Types.SPCINCIDENT_CONTROLOPTION ControlOption { get; set; }
        public bool Reviewed { get; set; }
        [MaxLength(500)]
        public string ClientFeedback { get; set; }
        public double FeedbackAnswer1 { get; set; }
        public double FeedbackAnswer2 { get; set; }
        #endregion
    }
}
