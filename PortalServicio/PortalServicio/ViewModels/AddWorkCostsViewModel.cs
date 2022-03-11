using Plugin.Connectivity;
using PortalAPI.Contracts;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class AddWorkCostsViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _Case;
        private MaterialYRepuestoViewModel _ToAdd;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private ObservableCollection<View> _Source;
        private int _SelectedPersonal;
        private int _Hours;
        private bool _IsBusy;
        private bool _IsInformationCorrect;            
        private readonly IPageService _pageService;
        private int _ProcessStep;
        private int _TotalSteps;
        private ObservableCollection<string> _AvailablePersonal;

        public ObservableCollection<string> AvailablePersonal
        {
            get { return _AvailablePersonal; }
            set { SetValue(ref _AvailablePersonal, value); }
        }
        public ObservableCollection<View> Source
        {
            get { return _Source; }
            set { SetValue(ref _Source, value); }
        }
        public int SelectedPersonal
        {
            get { return _SelectedPersonal; }
            set { SetValue(ref _SelectedPersonal, value); }
        }
        public int ProcessStep
        {
            get { return _ProcessStep; }
            set { SetValue(ref _ProcessStep, value); }
        }     
        public int TotalSteps
        {
            get { return _TotalSteps; }
            set { SetValue(ref _TotalSteps, value); }
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
            set { SetValue(ref _SelectedServiceTicket, value); }
        }
        public int Hours
        {
            get { return _Hours; }
            set { SetValue(ref _Hours, value); }
        }
        public ref ServiceTicketViewModel RefSelectedServiceTicket
        {
            get { return ref _SelectedServiceTicket; }
        }              
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }    
        public bool IsInformationCorrect
        {
            get { return _IsInformationCorrect; }
            set { SetValue(ref _IsInformationCorrect, value); }
        }

        public ICommand AddWorkCostsCommand { get; private set; }
        public ICommand ConfirmOperationCommand { get; private set; }
        #endregion

        #region Constructors
        public AddWorkCostsViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST)
        {
            _pageService = pageService;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            AvailablePersonal = new ObservableCollection<string>
            {
                "1 Técnico",
                "2 Técnicos",
                "1 Ingeniero / Jefe Técnico",
                "1 Ingeniero / Jefe Técnico + Ayudante"
            };
            Source = new ObservableCollection<View> { new Label { Text="Hola Mundo"} };
            ToAdd = new MaterialYRepuestoViewModel(new Models.MaterialYRepuesto {
                Count =1,
                Treatment =Types.SPCMATERIAL_TREATMENTOPTION.Cotizar  
            });
            TotalSteps=3;
            ProcessStep=0;
            SelectedPersonal = 0;
            Hours = 1;
            AddWorkCostsCommand = new Command(async () => await AddWorkCosts());
            ConfirmOperationCommand = new Command(async ()=> await AddWorkEstimated());
        }
        #endregion

        #region Events
        private async Task AddWorkCosts()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            ProcessStep= (ProcessStep+1)%TotalSteps;
            string cat = string.Empty;
            switch (Case.Client.Category.Name)
            {
                case "COMERCIAL":
                case "CORREO":
                    cat = "C";
                    break;
                case "FINANCIERO":
                    cat = "F";
                    break;
                case "INDUSTRIAL":
                    cat = "I";
                    break;
                case "RESIDENCIAL":
                    cat = "R";
                    break;
                case "Negocio Pequeño":
                    cat = "NP";
                    break;
                default:
                    break;
            }
            if (SelectedPersonal == -1)
                return;
            int hoursnumber = Hours;
            if(Hours>6)
                hoursnumber = 6;
            string currency = "L";
            if (SelectedServiceTicket.MoneyCurrency != null && SelectedServiceTicket.MoneyCurrency.Name.Equals("USD"))
                currency = "D";
            string code = string.Format("ST{0}{1}-{2}@{3}",cat,SelectedPersonal+1,hoursnumber,currency);
            ToAdd.Product = new ProductViewModel(await CRMConnector.GetTechnicalService(code));
            IsBusy = false;
        }

        private async Task AddWorkEstimated()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            if (!ValidateInformation())
            {
                await _pageService.DisplayAlert("Falta elegir mano de obra","Debe hacer recibido una opcion de cotizacion en esta ventana para poder agregarla como cotización de mano de obra.","Ok");
                IsBusy = false ;
                return;
            }
            try
            {
                MaterialYRepuesto matModel = ToAdd.ToModel();
                matModel.UnitPrice = Config.CalculatePrice(matModel.Product);
                if (CrossConnectivity.Current.IsConnected)
                    ToAdd = new MaterialYRepuestoViewModel(await CRMConnector.CreateNewMaterial(SelectedServiceTicket.SQLiteRecordId, matModel));
                else
                {
                    NotificationService.DisplayMessage("Sin internet", "Agregando localmente");
                    ToAdd = new MaterialYRepuestoViewModel(await CRMConnector.CreateNewMaterialOffline(SelectedServiceTicket.SQLiteRecordId, matModel));
                }
                SelectedServiceTicket.ProductsUsed.Add(ToAdd);
                await _pageService.PopUpPopAsync();
            }
            catch (Exception)
            {
                await _pageService.DisplayAlert("Error", "Ha sucedido un error al intentar agregar este producto. Repita la operación. Si el problema persiste contacte TI", "Ok");
            }
            IsBusy = false;
        }

        private bool ValidateInformation() => !(ToAdd == null || ToAdd.Count < 1 || ToAdd.Product == null);      
        #endregion
    }
}
