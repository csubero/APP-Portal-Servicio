using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class ProjectMaterial
    {
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Product Product { get; set; }
        [ForeignKey(typeof(Product))]
        public int ProductId { get; set; }
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        public int Quantity { get; set; }
        public int Claimed { get; set; }
        public int Remaining { get; set; }
        public int Requested { get; set; }
        public bool IsAdditional { get; set; }
    }
}
