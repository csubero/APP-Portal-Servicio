using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ProjectMaterialViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private ProductViewModel _Product;
        private int _ProductId;
        private int _CDTId;
        private int _Quantity;
        private int _Claimed;
        private int _Remaining;
        private int _Requested;
        private bool _IsAdditional;
        private bool _IsStillAvailable;

        public int SQLiteRecordId { get { return _SQLiteRecordId; }  set { SetValue(ref _SQLiteRecordId, value);  } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public ProductViewModel Product { get { return _Product; } set { SetValue(ref _Product, value); } }
        public int ProductId { get { return _ProductId; } set { SetValue(ref _ProductId, value); } }
        public int CDTId { get { return _CDTId; } set { SetValue(ref _CDTId, value); } }
        public int Quantity { get { return _Quantity; } set { SetValue(ref _Quantity, value); } }
        public int Claimed { get { return _Claimed; } set { SetValue(ref _Claimed, value); } }
        public int Remaining { get { return _Remaining; } set { SetValue(ref _Remaining, value); } }
        public int Requested { get { return _Requested; } set { SetValue(ref _Requested, value); } }
        public bool IsAdditional { get { return _IsAdditional; } set { SetValue(ref _IsAdditional, value); } }
        public bool IsStillAvailable { get { return _IsStillAvailable; } set { SetValue(ref _IsStillAvailable, value); } }
        public bool IsOutOfStock { get { return !_IsStillAvailable; } }
        #endregion
        #region Constructors
        public ProjectMaterialViewModel(ProjectMaterial projectMaterial)
        {
            if (projectMaterial == null)
                return;
            SQLiteRecordId = projectMaterial.SQLiteRecordId;
            InternalId = projectMaterial.InternalId;
            Product = new ProductViewModel(projectMaterial?.Product);
            CDTId = projectMaterial.CDTId;
            Claimed = projectMaterial.Claimed;
            IsAdditional = projectMaterial.IsAdditional;
            ProductId = projectMaterial.ProductId;
            Quantity = projectMaterial.Quantity;
            Remaining = projectMaterial.Remaining;
            Requested = projectMaterial.Requested;
            IsStillAvailable = Remaining != 0;
        }

        public ProjectMaterial ToModel() =>
            new ProjectMaterial
            {
                SQLiteRecordId = SQLiteRecordId,
                InternalId = InternalId,
                Product = Product?.ToModel(),
                CDTId = CDTId,
                Claimed = Claimed,
                IsAdditional = IsAdditional,
                ProductId = ProductId,
                Quantity = Quantity,
                Remaining = Remaining,
                Requested = Requested
            };
        #endregion
    }
}
