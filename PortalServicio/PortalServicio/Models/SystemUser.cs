using SQLite;
using System;

namespace PortalServicio.Models
{
    public class SystemUser
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(50),NotNull]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Token { get; set; }
    }
}
