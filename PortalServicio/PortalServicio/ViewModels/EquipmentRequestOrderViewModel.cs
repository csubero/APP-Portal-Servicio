using PortalServicio.Models;
using System;
using System.Collections.Generic;

namespace PortalServicio.ViewModels
{
    public class EquipmentRequestOrderViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private int _CDTId;
        private string _Number;
        private bool _IsApproved;
        private bool _IsCollapsed;
        private int _EquipmentRequestedHeight;
        private DateTime _ApprovedDate;
        List<LineEquipmentRequestOrderViewModel> _EquipmentRequested;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public int CDTId { get { return _CDTId; } set { SetValue(ref _CDTId, value); } }
        public string Number { get { return _Number; } set { SetValue(ref _Number, value); } }
        public bool IsApproved { get { return _IsApproved; } set { SetValue(ref _IsApproved, value); } }
        public bool IsCollapsed { get { return _IsCollapsed; } set { SetValue(ref _IsCollapsed, value); } }
        public int EquipmentRequestedHeight { get { return _EquipmentRequestedHeight; } set { SetValue(ref _EquipmentRequestedHeight, value); } }
        public DateTime ApprovedDate { get { return _ApprovedDate; } set { SetValue(ref _ApprovedDate, value); } }
        public List<LineEquipmentRequestOrderViewModel> EquipmentRequested { get { return _EquipmentRequested; } set { SetValue(ref _EquipmentRequested, value); } }
        #endregion

        #region Constructors
        public EquipmentRequestOrderViewModel(EquipmentRequestOrder order)
        {
            if (order == null)
                return;
            InternalId = order.InternalId;
            SQLiteRecordId = order.SQLiteRecordId;
            CDTId = order.CDTId;
            Number = order.Number;
            IsApproved = order.IsApproved;
            ApprovedDate = order.ApprovedDate;
            EquipmentRequested = new List<LineEquipmentRequestOrderViewModel>();
            if (order.EquipmentRequested != null)
                foreach (LineEquipmentRequestOrder line in order.EquipmentRequested)
                    EquipmentRequested.Add(new LineEquipmentRequestOrderViewModel(line));
            EquipmentRequestedHeight = 45 + EquipmentRequested.Count * 65;
        }

        public EquipmentRequestOrder ToModel()
        {
            List<LineEquipmentRequestOrder> EquipmentRequestedModel = new List<LineEquipmentRequestOrder>();
            foreach (LineEquipmentRequestOrderViewModel linevm in EquipmentRequested)
                EquipmentRequestedModel.Add(linevm.ToModel());
            return new EquipmentRequestOrder
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                CDTId = CDTId,
                ApprovedDate = ApprovedDate,
                IsApproved = IsApproved,
                Number = Number,
                EquipmentRequested = EquipmentRequestedModel
            }; 
        }
        #endregion
    }
}
