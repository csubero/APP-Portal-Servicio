using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class ExtraEquipmentRequest
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        [ForeignKey(typeof(Product))]
        public int EquipmentId { get; set; }
        public int Quantity { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Product Equipment { get; set; }
        public bool IsApproved { get; set; }
        [MaxLength(250)]
        public string Reason { get; set; }
        public Types.SPCEXTRAEQUIPMENT_PROCESSTYPE ProcessType { get; set; }
    }
}
