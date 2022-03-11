using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private string _Name;
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        private string _Id;
        public string Id { get { return _Id; } set { SetValue(ref _Id, value); } }
        private string _Bought;
        public string Bought { get { return _Bought; } set { SetValue(ref _Bought, value); } }
        private decimal _Cost;
        public decimal Cost { get { return _Cost; } set { SetValue(ref _Cost, value); } }
        private decimal _LawTax;
        public decimal LawTax { get { return _LawTax; } set { SetValue(ref _LawTax, value); } }
        private decimal _DAITax;
        public decimal DAITax { get { return _DAITax; } set { SetValue(ref _DAITax, value); } }
        private decimal _SelectiveTax;
        public decimal SelectiveTax { get { return _SelectiveTax; } set { SetValue(ref _SelectiveTax, value); } }
        private bool _DoesHaveMOSC;
        public bool DoesHaveMOSC { get { return _DoesHaveMOSC; } set { SetValue(ref _DoesHaveMOSC, value); } }
        #endregion

        #region Constructors
        public ProductViewModel(Product product)
        {
            if (product == null)
                return;
            InternalId = product.InternalId;
            SQLiteRecordId = product.SQLiteRecordId;
            Name = product.Name;
            Id = product.Id;
            DoesHaveMOSC = product.DoesHaveMOSC;
            DAITax = product.DAITax;
            LawTax = product.LawTax;
            SelectiveTax = product.SelectiveTax;
            Bought = product.Bought;
            Cost = product.Cost;
        }

        public Product ToModel() =>
            new Product
            {
                Id = Id,
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name,
                Cost = Cost,
                DAITax = DAITax,
                LawTax = LawTax,
                SelectiveTax = SelectiveTax,
                Bought = Bought,
                DoesHaveMOSC = DoesHaveMOSC
            };
        #endregion
    }
}
