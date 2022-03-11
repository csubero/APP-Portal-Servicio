using SQLite;
using System;

namespace PortalServicio.Models
{
    public class Company
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique, NotNull]
        public Guid InternalId { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        #endregion
    }
}
