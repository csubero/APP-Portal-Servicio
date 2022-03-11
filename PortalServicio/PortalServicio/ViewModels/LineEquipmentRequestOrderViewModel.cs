using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class LineEquipmentRequestOrderViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private ProductViewModel _Product;
        private int _Requested;
        private string _ProductCode;
        private int _EquipmentRequestOrderId;
        private int _ProductId;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public ProductViewModel Product { get { return _Product; } set { SetValue(ref _Product, value); } }
        public int EquipmentRequestOrderId { get { return _EquipmentRequestOrderId; } set { SetValue(ref _EquipmentRequestOrderId, value); } }
        public string ProductCode { get { return _ProductCode; } set { SetValue(ref _ProductCode, value); } }
        public int Requested { get { return _Requested; } set { SetValue(ref _Requested, value); } }
        public int ProductId { get { return _ProductId; } set { SetValue(ref _ProductId, value); } }
        #endregion

        #region Constructors
        public LineEquipmentRequestOrderViewModel(LineEquipmentRequestOrder line)
        {
            if (line == null)
                return;
            InternalId = line.InternalId;
            SQLiteRecordId = line.SQLiteRecordId;
            Product = line.Product != null ? new ProductViewModel(line.Product) : null;
            Requested = line.Requested;
            ProductCode = line.ProductCode;
            EquipmentRequestOrderId = line.EquipmentRequestOrderId;
            ProductId = line.ProductId;
        }

        public LineEquipmentRequestOrder ToModel() =>
            new LineEquipmentRequestOrder
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                EquipmentRequestOrderId = EquipmentRequestOrderId,
                Product = Product?.ToModel(),
                ProductId = ProductId,
                ProductCode = ProductCode,
                Requested = Requested
            };
        #endregion
    }
}
