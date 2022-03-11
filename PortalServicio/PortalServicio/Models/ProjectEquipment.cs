using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class ProjectEquipment
    {
        [PrimaryKey,AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        public int Quantity { get; set; }
        public int Claimed { get; set; }
        public int Remaining { get; set; }
        public int Requested { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Product Product { get; set; }
        [MaxLength(15),NotNull]
        public string Code { get; set; }
        public bool IsAdditional { get; set; }
        public bool IsCanceled { get; set; }
        //public string SPCPROJECTEQUIPMENT_SRE = "new_sre";
        [ForeignKey(typeof(CDT))]
        public int CDTId { get; set; }
        [ForeignKey(typeof(Product))]
        public int ProductId { get; set; }
    }
}
