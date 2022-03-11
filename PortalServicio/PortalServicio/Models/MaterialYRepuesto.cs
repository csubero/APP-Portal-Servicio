using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class MaterialYRepuesto
    {
        [ForeignKey(typeof(ServiceTicket))]
        public int ServiceTicketId { get; set; }
        [ForeignKey(typeof(Product))]
        public int ProductId { get; set; }

        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        public Guid InternalId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Product Product { get; set; }
        public decimal UnitPrice { get; set; }
        public int Count { get; set; }
        public Types.SPCMATERIAL_TREATMENTOPTION Treatment { get; set; }
        public Types.SPCMATERIAL_DESTINATIONOPTION Destination { get; set; }
        [MaxLength(50)]
        public string Serials { get; set; }
        [MaxLength(50)]
        public string RequestNumber { get; set; }
    }
}
