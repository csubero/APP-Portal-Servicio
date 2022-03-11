using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class LineEquipmentRequestOrder
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = ( CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert))]
        public Product Product { get; set; }
        public int Requested { get; set; }
        [MaxLength(25)]
        public string ProductCode { get; set; }
        [ForeignKey(typeof(EquipmentRequestOrder))]
        public int EquipmentRequestOrderId { get; set; }
        [ForeignKey(typeof(Product))]
        public int ProductId { get; set; }
    }
}
