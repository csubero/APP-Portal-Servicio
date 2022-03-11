using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PortalServicio.Models
{
    public class EquipmentRequestOrder
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        [MaxLength(10)]
        public string Number { get; set; }
        public bool IsApproved { get; set; }
        public DateTime ApprovedDate { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public List<LineEquipmentRequestOrder> EquipmentRequested { get; set; }
    }
}
