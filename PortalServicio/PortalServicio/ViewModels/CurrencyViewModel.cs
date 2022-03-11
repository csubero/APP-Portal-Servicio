using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class CurrencyViewModel : BaseViewModel
    {
        #region Properties
        private Guid _InternalId;
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        private int _SQLiteRecordId;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        private string _Symbol;
        public string Symbol { get { return _Symbol; } set { SetValue(ref _Symbol, value); } }
        private string _Name;
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        private string _Code;
        public string Code { get { return _Code; } set { SetValue(ref _Code, value); } }
        #endregion

        public CurrencyViewModel(Currency currency)
        {
            if (currency == null)
                return;
            SQLiteRecordId = currency.SQLiteRecordId;
            InternalId = currency.InternalId;
            Symbol = currency.Symbol;
            Name = currency.Name;
        }

        public Currency ToModel() =>
                new Currency
                {
                    InternalId = InternalId,
                    Name = Name,
                    Symbol = Symbol,
                    SQLiteRecordId = SQLiteRecordId,
                    Code = Code,
                };
    }
}
