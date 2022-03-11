using Plugin.Connectivity;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class ListCasesViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _SelectedIncident;
        private ObservableCollection<IncidentViewModel> _IncidentsObtained;
        private ObservableCollection<IncidentViewModel> _IncidentsFiltered;
        private string _SearchText;
        private bool _IsBusy;
        private bool _IsLoading;
        private readonly IPageService _pageService;

        public IncidentViewModel SelectedIncident
        {
            get { return _SelectedIncident; }
            set { SetValue(ref _SelectedIncident, value); }
        }
        public ObservableCollection<IncidentViewModel> IncidentsObtained
        {
            get { return _IncidentsObtained; }
            private set { SetValue(ref _IncidentsObtained, value); }
        }
        public ObservableCollection<IncidentViewModel> IncidentsFiltered
        {
            get { return _IncidentsFiltered; }
            private set { SetValue(ref _IncidentsFiltered, value); }
        }
        public string SearchText
        {
            get { return _SearchText; }
            set { SetValue(ref _SearchText, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsLoading
        {
            get { return _IsLoading; }
            set { SetValue(ref _IsLoading, value); }
        }

        public ICommand OpenIncidentCommand { get; private set; }
        public ICommand SearchIncidentsCommand { get; private set; }
        public ICommand AddBookmarkCommand { get; private set; }
        public ICommand DeleteBookmarkCommand { get; private set; }
        public ICommand FilterIncidentsCommand { get; private set; }
        public ICommand LoadIncidentsCommand { get; private set; }
        #endregion

        #region Constructors
        public ListCasesViewModel(IPageService pageService)
        {
            _pageService = pageService;
            OpenIncidentCommand = new Command(async () => await ClickIncident());
            SearchIncidentsCommand = new Command(async () => await SearchIncidents());
            FilterIncidentsCommand = new Command(FilterIncidents);
            LoadIncidentsCommand = new Command(async () => await LoadIncidents());
            LoadIncidents().ConfigureAwait(false);
        }
        #endregion

        #region Events
        /// <summary>
        /// Carga los incidentes del CRM.
        /// </summary>
        /// <returns></returns>
        private async Task LoadIncidents()
        {
            IsLoading = true;
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), Config.MSG_TITLE_LOAD_INCIDENTS, Config.MSG_LOAD_TECHSERVICE_PRODUCTS,0,2);
            await _pageService.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            try
            {        
                if (!CrossConnectivity.Current.IsConnected)
                {
                    vm.ProgressUp(Config.MSG_LOAD_LOCAL);
                    NotificationService.DisplayMessage("Sin internet", "Utilizando información local");
                    IncidentsObtained = new ObservableCollection<IncidentViewModel>(await CRMConnector.GetLocalIncidents());
                }
                else
                {
                    await CRMConnector.FetchTechnicalServiceProducts();
                    vm.ProgressUp(Config.MSG_LOAD_INCIDENTS);
                    IncidentsObtained = new ObservableCollection<IncidentViewModel>((await CRMConnector.GetIncidentsViewModel()).OrderByDescending(inc => inc.CreatedOn.Date).ThenByDescending(inc => inc.CreatedOn.TimeOfDay));
                }
                vm.ProgressUp(Config.MSG_LOAD_SUCCESS);
            }
            catch (HttpRequestException)
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar esta operación.", "Ok");
                    IsBusy = false;
                    return;
                }
                else
                    IncidentsObtained = new ObservableCollection<IncidentViewModel>((await CRMConnector.GetIncidentsViewModel()).OrderByDescending(inc => inc.CreatedOn.Date).ThenByDescending(inc => inc.CreatedOn.TimeOfDay));
            }
            //IncidentsObtained = new ObservableCollection<IncidentViewModel>(await RestService.GetIncidentsViewModel()); //WEBAPI
            IsLoading = false;
            FilterIncidents();
            vm.IsLoading = false;
        }

        private async Task ClickIncident()
        {
            if (SelectedIncident == null || IsBusy)
                return;
            IsBusy = true;
            Incident incident = null;
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                    incident = await CRMConnector.GetIncident(SelectedIncident.InternalId);
                else
                {
                    NotificationService.DisplayMessage("Sin internet", "Accesando información local.");
                    incident = await CRMConnector.GetLocalIncident(SelectedIncident.InternalId);
                }
            }
            catch (HttpRequestException)
            {
                IsBusy = false;
                if (!CrossConnectivity.Current.IsConnected)
                    await _pageService.DisplayAlert("Sin conexión", "Se ha detectado cambios en la red o falta de conexión a la misma. Reintente la operación", "Ok");
                SelectedIncident = null;
                return;
            }
            //catch (Exception ex)
            //{
            //    await _pageService.DisplayAlert("Error", ex.Message, "Ok");
            //}
            IsBusy = false;
            if (incident == null)
                NotificationService.DisplayMessage("No hay información", "No hay información local para cargar este caso. ");
            else
                await _pageService.PushAsync(new CasePage(new IncidentViewModel(incident)));
            SelectedIncident = null;
        }

        private async Task SearchIncidents()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                IsBusy = true;
                try
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar búsquedas de casos.", "Ok");
                        IsBusy = false;
                        return;
                    }
                    IncidentsObtained = new ObservableCollection<IncidentViewModel>((await CRMConnector.FindIncidentsViewModel(SearchText)).OrderByDescending(inc => inc.CreatedOn.Date).ThenByDescending(inc => inc.CreatedOn.TimeOfDay));
                }
                catch (HttpRequestException)
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar búsquedas de casos.", "Ok");
                        IsBusy = false;
                        return;
                    }
                    IncidentsObtained = new ObservableCollection<IncidentViewModel>((await CRMConnector.FindIncidentsViewModel(SearchText)).OrderByDescending(inc => inc.CreatedOn.Date).ThenByDescending(inc => inc.CreatedOn.TimeOfDay));
                }
                //IncidentsObtained = new ObservableCollection<IncidentViewModel>(await RestService.FindIncidentsViewModel(SearchText));
                IncidentsFiltered = IncidentsObtained;
                IsBusy = false;
            }
        }

        private void FilterIncidents()
        {
            if (IncidentsObtained != null)
                IncidentsFiltered = (string.IsNullOrEmpty(SearchText)) ? IncidentsObtained : new ObservableCollection<IncidentViewModel>(IncidentsObtained.Where(
                    inc => (
                        inc.TicketNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                        inc.Client.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                        inc.Client.Alias.ToUpper().Contains(SearchText.ToUpper())
                        )
                    ).OrderByDescending(inc => inc.CreatedOn.Date).ThenByDescending(inc => inc.CreatedOn.TimeOfDay)
                );
        }
        #endregion
    }
}
