using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ProductStorageViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private string _Name;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        #endregion

        #region Constructor
        public ProductStorageViewModel(ProductStorage productStorage)
        {
            if (productStorage == null)
                return;
            InternalId = productStorage.InternalId;
            SQLiteRecordId = productStorage.SQLiteRecordId;
            Name = productStorage.Name;
        }      

        public ProductStorage ToModel() =>
            new ProductStorage
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name
            };
        #endregion
    }
}
