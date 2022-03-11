using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class CreateTicketViewModel : BaseViewModel
    {
        #region Properties
        private string _SearchText;
        private IncidentViewModel _NewCase;
        private ObservableCollection<ClientViewModel> _PossibleClients;
        private ClientViewModel _SelectedClient;
        private bool _IsBusy;
        private bool _IsClientSelected;
        private bool _IsSearchDone;
        private readonly IPageService _pageService;

        public string SearchText
        {
            get { return _SearchText; }
            set { SetValue(ref _SearchText, value); }
        }
        public IncidentViewModel NewCase
        {
            get { return _NewCase; }
            set { SetValue(ref _NewCase, value); }
        }
        public ObservableCollection<ClientViewModel> PossibleClients
        {
            get { return _PossibleClients; }
            set { SetValue(ref _PossibleClients, value); }
        }
        public ClientViewModel SelectedClient
        {
            get { return _SelectedClient; }
            set { SetValue(ref _SelectedClient, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsClientSelected
        {
            get { return _IsClientSelected; }
            private set { SetValue(ref _IsClientSelected, value); }
        }
        public bool IsSearchDone
        {
            get { return _IsSearchDone; }
            private set { SetValue(ref _IsSearchDone, value); }
        }

        public ICommand CreateCaseCommand { get; private set; }
        public ICommand SelectClientCommand { get; private set; }
        public ICommand SearchClientCommand { get; private set; }
        #endregion

        #region Constructors
        public CreateTicketViewModel(IPageService pageService)
        {
            _pageService = pageService;
            NewCase = new IncidentViewModel(new Incident());
            CreateCaseCommand = new Command(async () => await CreateNewCase());
            SelectClientCommand = new Command(SelectClient);
            SearchClientCommand = new Command(async () => await SearchClient());
        }
        #endregion

        #region Events
        private async Task CreateNewCase()
        {
            if (IsBusy)
                return;
            if (!ValidateInformation())
            {
                NotificationService.DisplayMessage("Falta información", "Debe ingresar toda la información solicitada");
                return;
            }
            IsBusy = true;
            NotificationService.DisplayMessage("Creando", "Se está creando una boleta... Espera");
            NewCase = await CRMConnector.CreateNewTicket(NewCase);
            NotificationService.DisplayMessage("Creada", "Creación finalizada con éxito");
            IsBusy = false;
            await _pageService.PushAsync(new CasePage(NewCase));
            _pageService.RemovePages(2);
            //ServiceTicket created = await RestService.CreateNewServiceTicket(acase);
        }

        private void SelectClient()
        {
            if (IsBusy|| SelectedClient==null)
                return;           
            IsBusy = true;
            NewCase.Client = SelectedClient;             
            IsClientSelected = true;
            SelectedClient = null;
            IsBusy = false;
        }

        private bool ValidateInformation() => !(NewCase == null || string.IsNullOrEmpty(NewCase.Description) || string.IsNullOrEmpty(NewCase.Title) || NewCase.Client == null || NewCase.Client.InternalId.Equals(default(Guid)));

        private async Task SearchClient()
        {
            if (IsBusy)
                return;
            if (string.IsNullOrEmpty(SearchText))
            {
                await _pageService.DisplayAlert("Error de búsqueda", "Debe ingresar un valor para buscar al cliente.", "Ok");
                return;
            }
            IsBusy = true;
            IsSearchDone = false;
            PossibleClients = new ObservableCollection<ClientViewModel>(await CRMConnector.SearchClientAccounts(SearchText));
            IsSearchDone = true;
            IsBusy = false;
        }
        #endregion
    }
}
