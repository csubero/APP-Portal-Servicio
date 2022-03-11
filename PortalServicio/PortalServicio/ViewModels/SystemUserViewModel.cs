using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class SystemUserViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private string _Name;
        private string _Token;
        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public string Name { get { return _Name; } set { SetValue(ref _Name, value); } }
        public string Token { get { return _Token; } set { SetValue(ref _Token, value); } }
        #endregion

        #region Constructor
        public SystemUserViewModel(SystemUser systemUser)
        {
            if (systemUser == null)
                return;
            InternalId = systemUser.InternalId;
            SQLiteRecordId = systemUser.SQLiteRecordId;
            Name = systemUser.Name;
            Token = systemUser.Token;
        }

        public SystemUser ToModel() =>
            new SystemUser
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Name = Name,
                Token = Token
            };
        #endregion
    }
}
