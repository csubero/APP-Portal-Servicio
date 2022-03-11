using PortalAPI.Contracts;
using PortalAPI.DTO;
using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class ClientViewModel : BaseViewModel
    {
        #region Properties
        private int _SQLiteRecordId;
        public int SQLiteRecordId
        {
            get { return _SQLiteRecordId; }
            set { SetValue(ref _SQLiteRecordId, value); }
        }
        private int _CoordinatesId;
        public int CoordinatesId
        {
            get { return _CoordinatesId; }
            set { SetValue(ref _CoordinatesId, value); }
        }
        private int _CategoryId;
        public int CategoryId
        {
            get { return _CategoryId; }
            set { SetValue(ref _CategoryId, value); }
        }
        private Guid _InternalId;
        public Guid InternalId
        {
            get { return _InternalId; }
            set { SetValue(ref _InternalId, value); }
        }
        private Guid _PriceList;
        public Guid PriceList
        {
            get { return _PriceList; }
            set { SetValue(ref _PriceList, value); }
        }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }
        private string _Alias;
        public string Alias
        {
            get { return _Alias; }
            set { SetValue(ref _Alias, value); }
        }
        private string _Email;
        public string Email
        {
            get { return _Email; }
            set { SetValue(ref _Email, value); }
        }
        private string _Phone;
        public string Phone
        {
            get { return _Phone; }
            set { SetValue(ref _Phone, value); }
        }
        private string _Country;
        public string Country
        {
            get { return _Country; }
            set { SetValue(ref _Country, value); }
        }
        private Types.SPCCLIENT_REPORTTYPEOPTION _ReportType;
        public Types.SPCCLIENT_REPORTTYPEOPTION ReportType
        {
            get { return _ReportType; }
            set { SetValue(ref _ReportType, value); }
        }
        private bool _DoesPayTaxes;
        public bool DoesPayTaxes
        {
            get { return _DoesPayTaxes; }
            set { SetValue(ref _DoesPayTaxes, value); }
        }
        private CoordViewModel _Coordinates;
        public CoordViewModel Coordinates
        {
            get { return _Coordinates; }
            set { SetValue(ref _Coordinates, value); }
        }
        private CategoryViewModel _Category;
        public CategoryViewModel Category
        {
            get { return _Category; }
            set { SetValue(ref _Category, value); }
        }
        private string _Address;
        public string Address
        {
            get { return _Address; }
            set { SetValue(ref _Address, value); }
        }
        #endregion

        public ClientViewModel(Client client)
        {
            if (client == null)
                return;
            InternalId = client.InternalId;
            SQLiteRecordId = client.SQLiteRecordId;
            Name = client.Name;
            PriceList = client.PriceList;
            Alias = client.Alias;
            Email = client.Email;
            Phone = client.Phone;
            Country = client.Country;
            ReportType = client.ReportType;
            DoesPayTaxes = client.DoesPayTaxes;
            Coordinates = new CoordViewModel(client.Coordinates);
            CoordinatesId = client.CoordinatesId;
            Category = new CategoryViewModel(client.Category);
            CategoryId = client.CategoryId;
            Address = client.Address;
        }

        public ClientViewModel(DTO_ClientPartial client)
        {
            if (client == null)
                return;
            InternalId = client.InternalId;
            Name = client.Name;
            PriceList = client.PriceList;
            Alias = client.Alias;
        }

        public Client ToModel() =>
            new Client
            {
                InternalId = InternalId,
                SQLiteRecordId = SQLiteRecordId,
                Address = Address,
                Alias = Alias,
                Coordinates = Coordinates?.ToModel(),
                CoordinatesId = Coordinates.Id,
                Category = Category?.ToModel(),
                CategoryId = Category.SQLiteRecordId,
                Country = Country,
                DoesPayTaxes = DoesPayTaxes,
                Email = Email,
                Name = Name,
                Phone = Phone,
                PriceList = PriceList,
                ReportType = ReportType
            };
    }
}
