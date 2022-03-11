using SQLite;
using System;

namespace PortalServicio.Models
{
    public class ProductStorage
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(25), NotNull]
        public string Name { get; set; }
        #endregion

    }
}
