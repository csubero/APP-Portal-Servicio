using Plugin.Connectivity;
using PortalAPI.Contracts;
using PortalServicio.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class ListProgramCasesViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _SelectedIncident;
        private ObservableCollection<IncidentViewModel> _IncidentsObtained;
        private ObservableCollection<IncidentViewModel> _IncidentsFiltered;
        private string _SearchText;
        private bool _ProgrammingPending;
        private bool _Programmed;
        private bool _Reviewing;
        private bool _Reprogramming;
        private bool _Reprocess;
        private bool _List;
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
        public bool ProgrammingPending
        {
            get { return _ProgrammingPending; }
            set { SetValue(ref _ProgrammingPending, value); }
        }
        public bool Programmed
        {
            get { return _Programmed; }
            set { SetValue(ref _Programmed, value); }
        }
        public bool Reviewing
        {
            get { return _Reviewing; }
            set { SetValue(ref _Reviewing, value); }
        }
        public bool Reprogramming
        {
            get { return _Reprogramming; }
            set { SetValue(ref _Reprogramming, value); }
        }
        public bool Reprocess
        {
            get { return _Reprocess; }
            set { SetValue(ref _Reprocess, value); }
        }
        public bool List
        {
            get { return _List; }
            set { SetValue(ref _List, value); }
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
        public ICommand FilterControlCommand { get; private set; }
        public ICommand LoadIncidentsCommand { get; private set; }
        #endregion

        #region Constructors
        public ListProgramCasesViewModel(IPageService pageService)
        {
            _pageService = pageService;
            FilterControlCommand = new Command(FilterControl);
            LoadIncidentsCommand = new Command(async () => await LoadIncidents());
            OpenIncidentCommand = new Command(async () => await OpenIncident());
            ProgrammingPending = true;
            Reviewing = true;
            LoadIncidentsCommand?.Execute(null);
        }
        #endregion

        #region Events
        private async Task LoadIncidents()
        {
            IsLoading = true;
            IncidentsObtained = new ObservableCollection<IncidentViewModel>();
            IncidentsObtained = new ObservableCollection<IncidentViewModel>(await CRMConnector.GetIncidentsViewModelForProgramming());
            FilterControl();
            IsLoading = false;
        }

        private void FilterControl()
        {
            FilterIncidents();
            IncidentsFiltered = new ObservableCollection<IncidentViewModel>(IncidentsFiltered.
                Where(
                inc => (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.Undefined && ProgrammingPending) ||
                (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.Programado && Programmed) ||
                (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.EsperandoRevision && Reviewing) ||
                (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.PendienteCotizar && List) ||
                (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.Reprogramacion && Reprogramming) ||
                (inc.ControlOption == Types.SPCINCIDENT_CONTROLOPTION.Reproceso && Reprocess)
                ));
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

        private async Task OpenIncident()
        {
            if (SelectedIncident == null || IsBusy)
                return;
            IsBusy = true;
            IncidentViewModel incident = null;
            ObservableCollection<TechnicianViewModel> AvailableTechnicians = null;
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar abrir el caso.", "Ok");
                    IsBusy = false;
                    return;
                }
                incident = new IncidentViewModel(await CRMConnector.GetIncident(SelectedIncident.InternalId));
                AvailableTechnicians = await CRMConnector.GetTechniciansViewModel();
                for (int i = 0; i < 3; i++)
                    if (i < incident.TechniciansAssigned.Count)
                        incident.TechniciansAssigned[i] = AvailableTechnicians.Where((t) => t.InternalId == incident.TechniciansAssigned[i].InternalId).FirstOrDefault();
                    else
                        incident.TechniciansAssigned.Add(null);
            }
            catch (HttpRequestException)
            {
                IsBusy = false;
                await _pageService.DisplayAlert("Conexión Perdida", "Se ha detectado cambios en la red o falta de conexión a la misma. Reintente la operación", "Ok");
                SelectedIncident = null;
                return;
            }
            IsBusy = false;
            await _pageService.PushAsync(new ProgramCasePage(incident,AvailableTechnicians));
            SelectedIncident = null;
        }
        #endregion
    }
}
