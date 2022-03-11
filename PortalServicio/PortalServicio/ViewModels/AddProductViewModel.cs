using Plugin.Connectivity;
using PortalAPI.Contracts;
using PortalServicio.Configuration;
using PortalServicio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        #region Properties
        private string _SearchText;
        private IncidentViewModel _Case;
        private MaterialYRepuestoViewModel _ToAdd;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private ObservableCollection<ProductViewModel> _AvailableProducts;
        private ProductViewModel _SelectedProduct;
        private bool _IsBusy;
        private bool _IsProductSelected;
        private bool _IsDesmonteSelected;
        private bool _IsInformationCorrect;
        private Dictionary<string, Types.SPCMATERIAL_DESTINATIONOPTION> _DestinationDictionary;
        private Dictionary<string, Types.SPCMATERIAL_TREATMENTOPTION> _TreatmentDictionary;
        private readonly IPageService _pageService;

        public string SearchText
        {
            get { return _SearchText; }
            set { SetValue(ref _SearchText, value); }
        }
        public IncidentViewModel Case
        {
            get { return _Case; }
            set { SetValue(ref _Case, value); }
        }
        public MaterialYRepuestoViewModel ToAdd
        {
            get { return _ToAdd; }
            set { SetValue(ref _ToAdd, value); }
        }

        public ServiceTicketViewModel SelectedServiceTicket
        {
            get { return _SelectedServiceTicket; }
            set
            {
                SetValue(ref _SelectedServiceTicket, value);
            }
        }
        public ref ServiceTicketViewModel RefSelectedServiceTicket
        {
            get { return ref _SelectedServiceTicket; }
        }
        public ObservableCollection<ProductViewModel> AvailableProducts
        {
            get { return _AvailableProducts; }
            set { SetValue(ref _AvailableProducts, value); }
        }
        public ProductViewModel SelectedProduct
        {
            get { return _SelectedProduct; }
            set { SetValue(ref _SelectedProduct, value); IsInformationCorrect = ValidateInformation(); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public Dictionary<string, Types.SPCMATERIAL_DESTINATIONOPTION> DestinationDictionary
        {
            get { return _DestinationDictionary; }
            private set { SetValue(ref _DestinationDictionary, value); }
        }
        public List<KeyValuePair<string, Types.SPCMATERIAL_DESTINATIONOPTION>> DestinationList
        {
            get { return _DestinationDictionary.ToList(); }
        }
        public Dictionary<string, Types.SPCMATERIAL_TREATMENTOPTION> TreatmentDictionary
        {
            get { return _TreatmentDictionary; }
            private set { SetValue(ref _TreatmentDictionary, value); }
        }
        public List<KeyValuePair<string, Types.SPCMATERIAL_TREATMENTOPTION>> TreatmentList
        {
            get { return _TreatmentDictionary.ToList(); }
        }
        public bool IsProductSelected
        {
            get { return _IsProductSelected; }
            set { SetValue(ref _IsProductSelected, value); }
        }
        public bool IsDesmonteSelected
        {
            get { return _IsDesmonteSelected; }
            set { SetValue(ref _IsDesmonteSelected, value); }
        }
        public bool IsInformationCorrect
        {
            get { return _IsInformationCorrect; }
            set { SetValue(ref _IsInformationCorrect, value); }
        }

        public ICommand AddProductCommand { get; private set; }
        public ICommand SelectProductCommand { get; private set; }
        public ICommand SearchProductCommand { get; private set; }
        public ICommand MarkTreatmentCommand { get; private set; }
        public ICommand ValidateDestinationCommand { get; private set; }
        #endregion

        #region Constructors
        public AddProductViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST)
        {
            _pageService = pageService;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            DestinationDictionary = new Dictionary<string, Types.SPCMATERIAL_DESTINATIONOPTION>
            {
                { "Sitio del cliente", Types.SPCMATERIAL_DESTINATIONOPTION.Cliente },
                { "Hacia Taller", Types.SPCMATERIAL_DESTINATIONOPTION.Taller },
                { "Hacia Bodega", Types.SPCMATERIAL_DESTINATIONOPTION.Bodega }
            };
            TreatmentDictionary = new Dictionary<string, Types.SPCMATERIAL_TREATMENTOPTION>
            {
                { "Se factura", Types.SPCMATERIAL_TREATMENTOPTION.Facturar },
                { "Se cotiza", Types.SPCMATERIAL_TREATMENTOPTION.Cotizar },
                { "Se da soporte", Types.SPCMATERIAL_TREATMENTOPTION.Soporte },
                { "Se desmonta", Types.SPCMATERIAL_TREATMENTOPTION.Desmonte }
            };
            AvailableProducts = new ObservableCollection<ProductViewModel>();
            SearchProductCommand = new Command(async () => await SearchProducts());
            SelectProductCommand = new Command(SelectProduct);
            AddProductCommand = new Command(async () => await AddProduct());
            MarkTreatmentCommand = new Command(MarkTreatment);
            ValidateDestinationCommand = new Command(ValidateDestination);
        }
        #endregion

        #region Events
        private async Task SearchProducts()
        {
            IsBusy = true;
            AvailableProducts = new ObservableCollection<ProductViewModel>();
            ToAdd = null;
            IsProductSelected = false;
            string currency = "L";
            if (SelectedServiceTicket.MoneyCurrency != null && SelectedServiceTicket.MoneyCurrency.Name.Equals("USD"))
                currency = "D";
            if (CrossConnectivity.Current.IsConnected)
                AvailableProducts = await CRMConnector.GetProductsLikeExpression(SearchText, currency);
            else
                AvailableProducts = await CRMConnector.GetProductsLikeExpressionOffline(SearchText, currency);
            IsBusy = false;
        }

        private void SelectProduct()
        {
            if (SelectedProduct == null)
                return;
            IsBusy = true;
            ToAdd = new MaterialYRepuestoViewModel(new MaterialYRepuesto
            {
                Product = SelectedProduct.ToModel(),
                ProductId = SelectedProduct.SQLiteRecordId,        
                Count = 1
            });
            IsProductSelected = true;
            SelectedProduct = null;
            IsBusy = false;
        }

        private void MarkTreatment()
        {
            if (ToAdd != null && ToAdd.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Desmonte)
                IsDesmonteSelected = true;
            else
            {
                IsDesmonteSelected = false;
                ToAdd.Destination = Types.SPCMATERIAL_DESTINATIONOPTION.Undefined;
            }
            IsInformationCorrect = IsInformationCorrect = ValidateInformation();
        }

        private bool ValidateInformation() => !(ToAdd == null || ToAdd.Count < 1 || ToAdd.Product == null || ToAdd.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Undefined || (ToAdd.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Desmonte && ToAdd.Destination == Types.SPCMATERIAL_DESTINATIONOPTION.Undefined));

        private void ValidateDestination() =>
            IsInformationCorrect = ValidateInformation();

        private async Task AddProduct()
        {
            if (IsBusy)
                return;
            #region Input Validation
            if (ToAdd.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Undefined || (ToAdd.Treatment == Types.SPCMATERIAL_TREATMENTOPTION.Desmonte && ToAdd.Destination == Types.SPCMATERIAL_DESTINATIONOPTION.Undefined))
            {
                await _pageService.DisplayAlert("Falta llenar campos", "Debe ingresar información en los campos restantes para ingresar el artículo al sistema.", "Ok");
                return;
            }
            #endregion
            IsBusy = true;
            MaterialYRepuesto matModel = ToAdd.ToModel();
            matModel.UnitPrice = Config.CalculatePrice(matModel.Product);
            if (CrossConnectivity.Current.IsConnected)
            #region Online
            {
                try
                {
                    ToAdd = new MaterialYRepuestoViewModel(await CRMConnector.CreateNewMaterial(SelectedServiceTicket.SQLiteRecordId, matModel));
                }
                catch (Exception)
                {
                    await _pageService.DisplayAlert("Error", "Ha sucedido un error al intentar agregar este producto. Repita la operación. Si el problema persiste contacte TI", "Ok");
                }
            }
            #endregion
            else
            #region Offline
            {
                ToAdd = new MaterialYRepuestoViewModel(await CRMConnector.CreateNewMaterialOffline(SelectedServiceTicket.SQLiteRecordId, matModel));
            }
            #endregion
            SelectedServiceTicket.ProductsUsed.Add(ToAdd);
            await _pageService.PopUpPopAsync();
            IsBusy = false;
        }
        #endregion
    }
}
