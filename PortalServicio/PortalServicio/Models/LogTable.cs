using SQLite;
using System;

namespace PortalServicio.Models
{
    public class LogTable
    {
        [PrimaryKey]
        public string TableName { get; set; }
        public DateTime LastTimeUpdate { get; set; }
    }
}
