using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private string _Code;
        private string _Name;

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
        public string Code
        {
            get { return _Code; }
            set { SetValue(ref _Code, value); }
        }
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }
        #endregion

        #region Constructors
        public CategoryViewModel(Category cat)
        {
            if (cat == null)
                return;
            InternalId = cat.InternalId;
            SQLiteRecordId = cat.SQLiteRecordId;
            Code = cat.Code;
            Name = cat.Name;
        }

        public Category ToModel() =>
            new Category
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Code = Code,
                Name = Name
            };
        #endregion
    }
}
