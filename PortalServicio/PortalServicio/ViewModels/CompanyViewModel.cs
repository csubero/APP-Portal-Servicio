using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class CompanyViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        private Guid _InternalId;
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }
        #endregion

        public CompanyViewModel(Company company)
        {
            if (company == null)
                return;
            InternalId = company.InternalId;
            SQLiteRecordId = company.SQLiteRecordId;
            Name = company.Name;
        }

        public Company ToModel() =>
            new Company
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name
            };
    }
}
