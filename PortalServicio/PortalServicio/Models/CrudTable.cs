using System;
using PortalAPI.Contracts;
using SQLite;

namespace PortalServicio.Models
{
    public class CrudTable
    {      
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Types.CRUDOperation Action { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(25)]
        public string ObjectType { get; set; }
        [MaxLength(25)]
        public string AdditionalInfo { get; set; }
        public DateTime OperationDate { get; set; }
    }
}
