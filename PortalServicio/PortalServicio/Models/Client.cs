using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class Client
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique, NotNull]
        public Guid InternalId { get; set; }
        public Guid PriceList { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Alias { get; set; }
        [MaxLength(50)]
        public string Email { get; set; }
        [MaxLength(25)]
        public string Phone { get; set; }
        [MaxLength(25)]
        public string Country { get; set; }
        public Types.SPCCLIENT_REPORTTYPEOPTION ReportType { get; set; }
        public bool DoesPayTaxes { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public Coord Coordinates { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Category Category { get; set; }
        [MaxLength(200)]
        public string Address { get; set; }

        [ForeignKey(typeof(Coord))]
        public int CoordinatesId { get; set; }
        [ForeignKey(typeof(Category))]
        public int CategoryId { get; set; }
        #endregion     
    }
}
