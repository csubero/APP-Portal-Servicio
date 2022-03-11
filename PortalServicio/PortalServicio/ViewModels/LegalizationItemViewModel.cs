using PortalAPI.Contracts;
using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class LegalizationItemViewModel : BaseViewModel
    {
        public int _SQLiteRecordId;
        public Guid _InternalId;
        public CurrencyViewModel _Currency;
        public int _CurrencyId;
        public DateTime _SpentOn;
        public string _Bill;
        public bool _ProjectIssue;
        public int _LegalizationId;
        public decimal _Amount;
        public string _PaidTo;
        public Types.SPCLEGALIZATIONITEM_TYPE _ExpenseType;

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
        public CurrencyViewModel Currency
        {
            get { return _Currency; }
            set { SetValue(ref _Currency, value); }
        }
        public int CurrencyId
        {
            get { return _CurrencyId; }
            set { SetValue(ref _CurrencyId, value); }
        }
        public DateTime SpentOn
        {
            get { return _SpentOn; }
            set { SetValue(ref _SpentOn, value); }
        }
        public string Bill
        {
            get { return _Bill; }
            set { SetValue(ref _Bill, value); }
        }
        public bool ProjectIssue
        {
            get { return _ProjectIssue; }
            set { SetValue(ref _ProjectIssue, value); }
        }
        public int LegalizationId
        {
            get { return _LegalizationId; }
            set { SetValue(ref _LegalizationId, value); }
        }
        public decimal Amount
        {
            get { return _Amount; }
            set { SetValue(ref _Amount, value); }
        }
        public string PaidTo
        {
            get { return _PaidTo; }
            set { SetValue(ref _PaidTo, value); }
        }
        public Types.SPCLEGALIZATIONITEM_TYPE ExpenseType
        {
            get { return _ExpenseType; }
            set { SetValue(ref _ExpenseType, value); }
        }

        public LegalizationItemViewModel(LegalizationItem legItem)
        {
            if (legItem == null)
                return;
            SQLiteRecordId = legItem.SQLiteRecordId;
            InternalId = legItem.InternalId;
            Amount = legItem.Amount;
            Bill = legItem.Bill;
            Currency = legItem.Currency != null ? new CurrencyViewModel(legItem.Currency) : null;
            CurrencyId = legItem.CurrencyId;
            ExpenseType = legItem.ExpenseType;
            LegalizationId = legItem.LegalizationId;
            PaidTo = legItem.PaidTo;
            ProjectIssue = legItem.ProjectIssue;
            SpentOn = legItem.SpentOn;
        }

        public LegalizationItem ToModel() =>
            new LegalizationItem
            {
                Amount = Amount,
                Bill = Bill,
                Currency = Currency?.ToModel(),
                CurrencyId = CurrencyId,
                ExpenseType = ExpenseType,
                InternalId = InternalId,
                LegalizationId = LegalizationId,
                PaidTo = PaidTo,
                ProjectIssue = ProjectIssue,
                SpentOn = SpentOn,
                SQLiteRecordId = SQLiteRecordId
            };
    }
}
