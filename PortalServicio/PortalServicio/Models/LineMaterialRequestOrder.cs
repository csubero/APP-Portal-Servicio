using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class LineMaterialRequestOrder
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = (CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert))]
        public Product Material { get; set; }
        public int Requested { get; set; }
        [MaxLength(25)]
        public string MaterialCode { get; set; }
        [ForeignKey(typeof(MaterialRequestOrder))]
        public int MaterialRequestOrderId { get; set; }
        [ForeignKey(typeof(Product))]
        public int MaterialId { get; set; }
    }
}
