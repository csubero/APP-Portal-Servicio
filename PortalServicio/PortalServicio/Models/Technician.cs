using PortalAPI.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace PortalServicio.Models
{
    public class Technician : IComparable<Technician>
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int SQLiteRecordId { get; set; }
        [Unique]
        public Guid InternalId { get; set; }
        [MaxLength(50),NotNull]
        public string Name { get; set; }
        [OneToOne( CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead )]
        public SystemUser RelatedUser { get; set; }
        [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public ProductStorage ProductStorage { get; set; }
        public Types.SPCTECHNICIAN_CATEGORY Category { get; set; }

        [ForeignKey(typeof(SystemUser))]
        public int RelatedUserId { get; set; }
        [ForeignKey(typeof(ProductStorage))]
        public int ProductStorageId { get; set; }
        #endregion

        #region Methods       
        public int CompareTo(Technician t2)
        {
            if (t2 == null)
                return 1;
            return Name.CompareTo(t2.Name);
        }
        #endregion
    }
}
