using System;
using SQLite;

namespace PortalServicio.Models
{
    public class Currency
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(5),NotNull]
        public string Symbol { get; set; }
        [MaxLength(5)]
        public string Code { get; set; }
        [MaxLength(10),NotNull]
        public string Name { get; set; }
        #endregion
    }
}
