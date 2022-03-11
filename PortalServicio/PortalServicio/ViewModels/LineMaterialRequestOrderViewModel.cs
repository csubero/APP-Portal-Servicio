using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class LineMaterialRequestOrderViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private ProductViewModel _Material;
        private int _Requested;
        private string _MaterialCode;
        private int _MaterialRequestOrderId;
        private int _MaterialId;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public ProductViewModel Material { get { return _Material; } set { SetValue(ref _Material, value); } }
        public int MaterialRequestOrderId { get { return _MaterialRequestOrderId; } set { SetValue(ref _MaterialRequestOrderId, value); } }
        public string MaterialCode { get { return _MaterialCode; } set { SetValue(ref _MaterialCode, value); } }
        public int Requested { get { return _Requested; } set { SetValue(ref _Requested, value); } }
        public int MaterialId { get { return _MaterialId; } set { SetValue(ref _MaterialId, value); } }
        #endregion

        #region Constructors
        public LineMaterialRequestOrderViewModel(LineMaterialRequestOrder line)
        {
            if (line == null)
                return;
            InternalId = line.InternalId;
            SQLiteRecordId = line.SQLiteRecordId;
            Material = line.Material != null ? new ProductViewModel(line.Material) : null;
            Requested = line.Requested;
            MaterialCode = line.MaterialCode;
            MaterialRequestOrderId = line.MaterialRequestOrderId;
            MaterialId = line.MaterialId;
        }

        public LineMaterialRequestOrder ToModel() =>
            new LineMaterialRequestOrder
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                MaterialRequestOrderId = MaterialRequestOrderId,
                Material = Material?.ToModel(),
                MaterialId = MaterialId,
                MaterialCode = MaterialCode,
                Requested = Requested
            };
        #endregion
    }
}
