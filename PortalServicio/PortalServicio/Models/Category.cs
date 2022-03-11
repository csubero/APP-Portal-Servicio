using SQLite;
using System;

namespace PortalServicio.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(10), NotNull]
        public string Code { get; set; }
        [MaxLength(20), NotNull]
        public string Name { get; set; }
    }
}
