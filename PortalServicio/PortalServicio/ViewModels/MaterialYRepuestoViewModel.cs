using PortalAPI.Contracts;
using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class MaterialYRepuestoViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private int _ProductId;
        public int ProductId { get { return _ProductId; } set { SetValue(ref _ProductId, value); } }
        private int _ServiceTicketId;
        public int ServiceTicketId { get { return _ServiceTicketId; } set { SetValue(ref _ServiceTicketId, value); } }
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private ProductViewModel _Product;
        public ProductViewModel Product { get { return _Product; } set { SetValue(ref _Product, value); } }
        private decimal _UnitPrice;
        public decimal UnitPrice { get { return _UnitPrice; } set { SetValue(ref _UnitPrice, value); } }
        private int _Count;
        public int Count { get { return _Count; } set { SetValue(ref _Count, value); } }
        private string _RequestNumber;
        public string RequestNumber
        {
            get { return _RequestNumber; }
            set { SetValue(ref _RequestNumber, value); }
        }
        private string _serials;
        public string Serials { get { return _serials; } set { SetValue(ref _serials, value); } }
        private Types.SPCMATERIAL_TREATMENTOPTION _Treatment;
        public Types.SPCMATERIAL_TREATMENTOPTION Treatment { get { return _Treatment; } set { SetValue(ref _Treatment, value); } }
        private Types.SPCMATERIAL_DESTINATIONOPTION _Destination;
        public Types.SPCMATERIAL_DESTINATIONOPTION Destination { get { return _Destination; } set { SetValue(ref _Destination, value); } }
        #endregion

        #region Constructors
        public MaterialYRepuestoViewModel(MaterialYRepuesto material)
        {
            if (material == null)
                return;
            InternalId = material.InternalId;
            SQLiteRecordId = material.SQLiteRecordId;
            ProductId = material.ProductId;
            ServiceTicketId = material.ServiceTicketId;
            UnitPrice = material.UnitPrice;
            Count = material.Count;
            if (material.Product != null)
                Product = new ProductViewModel(material.Product);
            Serials = material.Serials;
            RequestNumber = material.RequestNumber;
            Treatment = material.Treatment;
            Destination = material.Destination;
        }

        public MaterialYRepuesto ToModel() =>
            new MaterialYRepuesto
            {
                ServiceTicketId = ServiceTicketId,
                Count = Count,
                Destination = Destination,
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Product = Product?.ToModel(),
                ProductId = Product?.SQLiteRecordId?? 0,
                Serials = Serials,
                RequestNumber = RequestNumber,
                Treatment = Treatment,
                UnitPrice = UnitPrice
            };
        #endregion
    }
}
