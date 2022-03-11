using PortalAPI.Contracts;
using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class TechnicianViewModel : BaseViewModel
    {
        #region Properties
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private int _ProductStorageId;
        public int ProductStorageId { get { return _ProductStorageId; } set { SetValue(ref _ProductStorageId, value); } }
        private int _RelatedUserId;
        public int RelatedUserId { get { return _RelatedUserId; } set { SetValue(ref _RelatedUserId, value); } }
        private string _Name;
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        private Types.SPCTECHNICIAN_CATEGORY _Category;
        public Types.SPCTECHNICIAN_CATEGORY Category { get { return _Category; } set { SetValue(ref _Category, value); } }

        private SystemUser _RelatedUser;
        public SystemUser RelatedUser { get { return _RelatedUser; } set { SetValue(ref _RelatedUser, value); } }
        private ProductStorage _ProductStorage;
        public ProductStorage ProductStorage { get { return _ProductStorage; } set { SetValue(ref _ProductStorage, value); } }
        #endregion

        public TechnicianViewModel(Technician technician)
        {
            if (technician == null)
                return;
            InternalId = technician.InternalId;
            SQLiteRecordId = technician.SQLiteRecordId;
            ProductStorage = technician.ProductStorage;
            ProductStorageId = technician.ProductStorageId;
            RelatedUser = technician.RelatedUser;
            RelatedUserId = technician.RelatedUserId;
            Name = technician.Name;
            ProductStorageId = technician.ProductStorageId;
        }

        public Technician ToModel() =>
            new Technician
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name,
                ProductStorageId = ProductStorageId,
                RelatedUser = RelatedUser,
                ProductStorage = ProductStorage,
                RelatedUserId = RelatedUserId,
            };
    }
}
