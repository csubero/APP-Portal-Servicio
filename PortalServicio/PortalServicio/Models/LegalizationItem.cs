using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class LegalizationItem
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Currency Currency { get; set; }
        [ForeignKey(typeof(Currency))]
        public int CurrencyId { get; set; }
        public DateTime SpentOn { get; set; }
        [MaxLength(25), NotNull]
        public string Bill { get; set; }
        public bool ProjectIssue { get; set; }
        [ForeignKey(typeof(Legalization))]
        public int LegalizationId { get; set; }
        public decimal Amount { get; set; }
        [NotNull]
        public string PaidTo { get; set; }
        public Types.SPCLEGALIZATIONITEM_TYPE ExpenseType { get; set; }
    }
}
