using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ProjectEquipmentViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private int _Quantity;
        private int _Claimed;
        private int _Remaining;
        private int _Requested;
        private ProductViewModel _Product;
        private string _Code;
        private bool _IsAdditional;
        private bool _IsCanceled;
        private bool _IsStillAvailable;
        private int _CDTId;
        private int _ProductId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public int Quantity { get { return _Quantity; } set { SetValue(ref _Quantity, value); } }
        public int Claimed { get { return _Claimed; } set { SetValue(ref _Claimed, value); } }
        public int Remaining { get { return _Remaining; } set { SetValue(ref _Remaining, value); } }
        public int Requested { get { return _Requested; } set { SetValue(ref _Requested, value); } }
        public ProductViewModel Product { get { return _Product; } set { SetValue(ref _Product, value); } }
        public string Code { get { return _Code; } set { SetValue(ref _Code, value); } }
        public bool IsAdditional { get { return _IsAdditional; } set { SetValue(ref _IsAdditional, value); } }
        public bool IsCanceled { get { return _IsCanceled; } set { SetValue(ref _IsCanceled, value); } }
        public bool IsStillAvailable { get { return _IsStillAvailable; } set { SetValue(ref _IsStillAvailable, value); } }
        public bool IsOutOfStock
        {
            get { return !_IsStillAvailable; }
        }
        public int CDTId { get { return _CDTId; } set { SetValue(ref _CDTId, value); } }
        public int ProductId { get { return _ProductId; } set { SetValue(ref _ProductId, value); } }
        #endregion
        #region Constructors
        public ProjectEquipmentViewModel(ProjectEquipment projectEquipment)
        {
            if (projectEquipment == null)
                return;
            SQLiteRecordId = projectEquipment.SQLiteRecordId;
            InternalId = projectEquipment.InternalId;
            Product = new ProductViewModel(projectEquipment.Product);
            CDTId = projectEquipment.CDTId;
            Claimed = projectEquipment.Claimed;
            Code = projectEquipment.Code;
            IsAdditional = projectEquipment.IsAdditional;
            IsCanceled = projectEquipment.IsCanceled;
            ProductId = projectEquipment.ProductId;
            Quantity = projectEquipment.Quantity;
            Remaining = projectEquipment.Remaining;
            Requested = projectEquipment.Requested;
            IsStillAvailable = Remaining != 0;
        }

        public ProjectEquipment ToModel() =>
            new ProjectEquipment
            {
                SQLiteRecordId = SQLiteRecordId,
                InternalId = InternalId,
                Product = Product?.ToModel(),
                CDTId = CDTId,
                Claimed = Claimed,
                Code = Code,
                IsAdditional = IsAdditional,
                IsCanceled = IsCanceled,
                ProductId = ProductId,
                Quantity = Quantity,
                Remaining = Remaining,
                Requested = Requested
            };
        #endregion
    }
}
