using Plugin.Connectivity;
using PortalAPI.Contracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class AddExtraEquipmentViewModel : BaseViewModel
    {
        #region Properties
        private CDTViewModel _CDT;
        private ExtraEquipmentRequestViewModel _ToAdd;
        private int _Phase;
        private bool _IsBusy;
        private bool _IsFreeSelected;
        private bool _IsProcessTypeSelected;
        private readonly IPageService _pageService;
        private string _SearchText;
        private ObservableCollection<ProductViewModel> _AvailableProducts;
        private ProductViewModel _SelectedProduct;
        private bool _IsProductSelected;
        private Dictionary<string, Types.SPCEXTRAEQUIPMENT_PROCESSTYPE> _ProcessTypeDictionary;

        public CDTViewModel CDT
        {
            get { return _CDT; }
            set { SetValue(ref _CDT, value); }
        }
        public ExtraEquipmentRequestViewModel ToAdd
        {
            get { return _ToAdd; }
            set { SetValue(ref _ToAdd, value); }
        }
        public int Phase
        {
            get { return _Phase; }
            private set { SetValue(ref _Phase, value); }
        }
        public ref CDTViewModel RefCDT
        {
            get { return ref _CDT; }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsFreeSelected
        {
            get { return _IsFreeSelected; }
            private set { SetValue(ref _IsFreeSelected, value); }
        }
        public bool IsProcessTypeSelected
        {
            get { return _IsProcessTypeSelected; }
            private set { SetValue(ref _IsProcessTypeSelected, value); }
        }
        public string SearchText
        {
            get { return _SearchText; }
            set { SetValue(ref _SearchText, value); }
        }
        public ObservableCollection<ProductViewModel> AvailableProducts
        {
            get { return _AvailableProducts; }
            set { SetValue(ref _AvailableProducts, value); }
        }
        public ProductViewModel SelectedProduct
        {
            get { return _SelectedProduct; }
            set { SetValue(ref _SelectedProduct, value); }
        }
        public Dictionary<string, Types.SPCEXTRAEQUIPMENT_PROCESSTYPE> ProcessTypeDictionary
        {
            get { return _ProcessTypeDictionary; }
            private set { SetValue(ref _ProcessTypeDictionary, value); }
        }
        public List<KeyValuePair<string, Types.SPCEXTRAEQUIPMENT_PROCESSTYPE>> ProcessTypeList
        {
            get { return _ProcessTypeDictionary.ToList(); }
        }
        public bool IsProductSelected
        {
            get { return _IsProductSelected; }
            set { SetValue(ref _IsProductSelected, value); }
        }

        public ICommand AddExtraEquipmentCommand { get; private set; }
        public ICommand SelectProductCommand { get; private set; }
        public ICommand SearchProductCommand { get; private set; }
        public ICommand MarkTreatmentCommand { get; private set; }
        public ICommand ValidateProcessTypeCommand { get; private set; }
        public ICommand NextPhaseCommand { get; private set; }
        public ICommand PreviousPhaseCommand { get; private set; }
        #endregion

        #region Constructors
        public AddExtraEquipmentViewModel(IPageService pageService, ref CDTViewModel cdt)
        {
            _pageService = pageService;
            CDT = cdt;
            ProcessTypeDictionary = new Dictionary<string, Types.SPCEXTRAEQUIPMENT_PROCESSTYPE>
            {
                { "Pedido", Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Order },
                { "Oferta", Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Offer },
                { "No se cobra", Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Free }
            };
            AvailableProducts = new ObservableCollection<ProductViewModel>();
            Phase = 0;
            IsProcessTypeSelected = false;
            //LoadTechnicians().ConfigureAwait(true);
            //ToAdd = new ExtraEquipmentRequestViewModel(new Models.ExtraEquipmentRequest());
            SearchProductCommand = new Command(async () => await SearchProducts());
            AddExtraEquipmentCommand = new Command(async () => await AddExtraEquipment());
            ValidateProcessTypeCommand = new Command(ValidateProcessType);
            SelectProductCommand = new Command(SelectProduct);
            NextPhaseCommand = new Command(NextPhase);
            PreviousPhaseCommand = new Command(PreviousPhase);
        }
        #endregion

        //    private async Task LoadTechnicians() =>
        //AvailableTechnicians = await CRMConnector.GetTechniciansViewModel();

        private async Task AddExtraEquipment()
        {
            IsBusy = true;
            var reg = await CRMConnector.CreateNewExtraEquipment(ToAdd.ToModel(), CDT.ToModel());
            RefCDT.ExtraEquipment.Add(new ExtraEquipmentRequestViewModel(reg));
            IsBusy = false;
            await _pageService.PopUpPopAsync();
        }

        private void ValidateProcessType()
        {
            IsFreeSelected = ToAdd.ProcessType == Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Free;
            IsProcessTypeSelected = ToAdd.ProcessType != Types.SPCEXTRAEQUIPMENT_PROCESSTYPE.Undefined;
        }

        #region Events
        private async Task SearchProducts()
        {
            IsBusy = true;
            AvailableProducts = new ObservableCollection<ProductViewModel>();
            ToAdd = null;
            SelectedProduct = null;
            IsProductSelected = false;
            string currency = "D";
            if (CrossConnectivity.Current.IsConnected)
                AvailableProducts = await CRMConnector.GetProductsLikeExpression(SearchText, currency);
            else
                AvailableProducts = await CRMConnector.GetProductsLikeExpressionOffline(SearchText, currency);
            IsBusy = false;
        }

        private void NextPhase() =>
            Phase += !IsFreeSelected && Phase == 2 ? 2 : 1;

        private void PreviousPhase() =>
            Phase -= !IsFreeSelected && Phase == 4 ? 2 : 1;

        private void SelectProduct()
        {
            if (SelectedProduct == null)
                return;
            IsBusy = true;
            ToAdd = new ExtraEquipmentRequestViewModel(new Models.ExtraEquipmentRequest
            {
                Equipment = SelectedProduct.ToModel(),
                EquipmentId = SelectedProduct.SQLiteRecordId,
                Quantity = 1
            });
            IsProductSelected = true;
            //SelectedProduct = null;
            IsBusy = false;
        }
        #endregion
    }
}