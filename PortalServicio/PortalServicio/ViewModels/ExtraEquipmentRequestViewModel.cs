using PortalAPI.Contracts;
using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ExtraEquipmentRequestViewModel : BaseViewModel
    {
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private int _CDTId;
        private int _EquipmentId;
        private int _Quantity;
        private ProductViewModel _Equipment;
        private bool _IsApproved;
        private string _Reason;
        private Types.SPCEXTRAEQUIPMENT_PROCESSTYPE _ProcessType;

        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        public int CDTId
        {
            get { return _CDTId; }
            set { SetValue(ref _CDTId, value); }
        }
        public int EquipmentId
        {
            get { return _EquipmentId; }
            set { SetValue(ref _EquipmentId, value); }
        }
        public int Quantity
        {
            get { return _Quantity; }
            set { SetValue(ref _Quantity, value); }
        }
        public ProductViewModel Equipment
        {
            get { return _Equipment; }
            set { SetValue(ref _Equipment, value); }
        }
        public bool IsApproved
        {
            get { return _IsApproved; }
            set { SetValue(ref _IsApproved, value); }
        }
        public string Reason
        {
            get { return _Reason; }
            set { SetValue(ref _Reason, value); }
        }
        public Types.SPCEXTRAEQUIPMENT_PROCESSTYPE ProcessType
        {
            get { return _ProcessType; }
            set { SetValue(ref _ProcessType, value); }
        }

        public ExtraEquipmentRequestViewModel(ExtraEquipmentRequest EER)
        {
            if (EER == null)
                return;
            InternalId = EER.InternalId;
            SQLiteRecordId = EER.SQLiteRecordId;
            Equipment = new ProductViewModel(EER.Equipment);
            EquipmentId = EER.EquipmentId;
            Reason = EER.Reason;
            IsApproved = EER.IsApproved;
            CDTId = EER.CDTId;
            Quantity = EER.Quantity;
            ProcessType = EER.ProcessType;
        }

        public ExtraEquipmentRequest ToModel() =>
            new ExtraEquipmentRequest
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Equipment = Equipment?.ToModel(),
                EquipmentId = EquipmentId,
                CDTId = CDTId,
                IsApproved = IsApproved,
                ProcessType = ProcessType,
                Quantity = Quantity,
                Reason = Reason
            };
    }
}
