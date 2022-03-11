using PortalServicio.Models;
using System;
using System.Collections.Generic;

namespace PortalServicio.ViewModels
{
    public class MaterialRequestOrderViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private int _CDTId;
        private string _Number;
        private bool _IsApproved;
        private bool _IsCollapsed;
        private int _MaterialsRequestedHeight;
        private DateTime _ApprovedDate;
        List<LineMaterialRequestOrderViewModel> _MaterialsRequested;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public int CDTId { get { return _CDTId; } set { SetValue(ref _CDTId, value); } }
        public string Number { get { return _Number; } set { SetValue(ref _Number, value); } }
        public bool IsApproved { get { return _IsApproved; } set { SetValue(ref _IsApproved, value); } }
        public bool IsCollapsed { get { return _IsCollapsed; } set { SetValue(ref _IsCollapsed, value); } }
        public int MaterialsRequestedHeight { get { return _MaterialsRequestedHeight; } set { SetValue(ref _MaterialsRequestedHeight, value); } }
        public DateTime ApprovedDate { get { return _ApprovedDate; } set { SetValue(ref _ApprovedDate, value); } }
        public List<LineMaterialRequestOrderViewModel> MaterialsRequested { get { return _MaterialsRequested; } set { SetValue(ref _MaterialsRequested, value); } }
        #endregion

        #region Constructors
        public MaterialRequestOrderViewModel(MaterialRequestOrder order)
        {
            if (order == null)
                return;
            InternalId = order.InternalId;
            SQLiteRecordId = order.SQLiteRecordId;
            CDTId = order.CDTId;
            Number = order.Number;
            IsApproved = order.IsApproved;
            ApprovedDate = order.ApprovedDate;
            MaterialsRequested = new List<LineMaterialRequestOrderViewModel>();
            if (order.MaterialsRequested != null)
                foreach (LineMaterialRequestOrder line in order.MaterialsRequested)
                    MaterialsRequested.Add(new LineMaterialRequestOrderViewModel(line));
            MaterialsRequestedHeight = 45 + _MaterialsRequested.Count * 60;
        }

        public MaterialRequestOrder ToModel()
        {
            List<LineMaterialRequestOrder> MaterialsRequestedModel = new List<LineMaterialRequestOrder>();
            foreach (LineMaterialRequestOrderViewModel linevm in MaterialsRequested)
                MaterialsRequestedModel.Add(linevm.ToModel());
            return new MaterialRequestOrder
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                CDTId = CDTId,
                ApprovedDate = ApprovedDate,
                IsApproved = IsApproved,
                Number = Number,
                MaterialsRequested = MaterialsRequestedModel
            };
        }
        #endregion
    }
}
