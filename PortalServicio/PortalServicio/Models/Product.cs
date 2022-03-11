using SQLite;
using System;

namespace PortalServicio.Models
{
    public class Product
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(100),NotNull]
        public string Name { get; set; }
        [MaxLength(50),NotNull]
        public string Id { get; set; }
        [MaxLength(10)]
        public string Bought { get; set; }
        public decimal Cost { get; set; }
        public decimal LawTax { get; set; }
        public decimal DAITax { get; set; }
        public decimal SelectiveTax { get; set; }
        public bool DoesHaveMOSC { get; set; }
        #endregion    
    }
}
