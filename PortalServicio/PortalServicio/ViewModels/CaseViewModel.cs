using Plugin.Connectivity;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Connectivity;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class CaseViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _Case;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private bool _IsBusy;
        private bool _IsRefreshing;
        private bool _DoesHaveCoords;
        private int _ServiceTicketsHeight;
        private readonly IPageService _pageService;

        public IncidentViewModel Case
        {
            get { return _Case; }
            set
            {
                SetValue(ref _Case, value);
                DoesHaveCoords = Case.Client.Coordinates.Latitude != 0 && Case.Client.Coordinates.Longitude != 0;
                ServiceTicketsHeight = Case.ServiceTickets.Count * 50;
            }
        }
        public ref IncidentViewModel RefCase
        {
            get { return ref _Case; }
        }
        public ServiceTicketViewModel SelectedServiceTicket
        {
            get { return _SelectedServiceTicket; }
            set { SetValue(ref _SelectedServiceTicket, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public bool IsRefreshing
        {
            get { return _IsRefreshing; }
            private set { SetValue(ref _IsRefreshing, value); }
        }
        public bool DoesHaveCoords
        {
            get { return _DoesHaveCoords; }
            private set { SetValue(ref _DoesHaveCoords, value); }
        }
        public int ServiceTicketsHeight
        {
            get { return _ServiceTicketsHeight; }
            set { SetValue(ref _ServiceTicketsHeight, value); }
        }

        public ICommand OpenServiceTicketCommand { get; private set; }
        public ICommand CreateNewServiceTicketCommand { get; private set; }
        public ICommand OpenWazeCommand { get; private set; }
        public ICommand DeleteServiceTicketCommand { get; private set; }
        public ICommand ReloadCaseCommand { get; private set; }
        #endregion

        #region Constructors
        public CaseViewModel(IPageService pageService, IncidentViewModel incident)
        {
            _pageService = pageService;
            Case = incident;
            OpenServiceTicketCommand = new Command<ServiceTicketViewModel>(async (stvm) => await OpenServiceTicket(stvm));
            CreateNewServiceTicketCommand = new Command(async () => await CreateNewServiceTicket());
            OpenWazeCommand = new Command(OpenWaze);
            DeleteServiceTicketCommand = new Command<ServiceTicketViewModel>(async (sel) => await DeleteServiceTicket(sel));
            ReloadCaseCommand = new Command(async () => await ReloadCase());
        }
        #endregion

        #region Events
        /// <summary>
        /// Recarga el caso con la información más actualizada si posee internet o con la ultima información guardada localmente si se esta desconectado a internet.
        /// </summary>
        /// <returns>Void</returns>
        private async Task ReloadCase()
        {
            if (IsRefreshing)
                return;
            IsRefreshing = true;
            if (CrossConnectivity.Current.IsConnected)
                Case = new IncidentViewModel(await CRMConnector.GetIncident(Case.InternalId));
            else
            {
                NotificationService.DisplayMessage("Sin internet", "Accesando información local.");
                Case = new IncidentViewModel(await CRMConnector.GetLocalIncident(Case.InternalId));
            }
            IsRefreshing = false;
        }

        /// <summary>
        /// Crea una nueva boleta de servicio, ya sea con internet o sin él.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreateNewServiceTicket()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            NotificationService.DisplayMessage("Creando", "Se está creando una boleta... Espera");
            #region Check Permissions
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                if (results.ContainsKey(Permission.Location))
                    status = results[Permission.Location];
            }
            #endregion     
            if (status == PermissionStatus.Granted)
            {
                #region Get Coordinates
                int meters = 10000;
                Tuple<double, double> currentLocation = new Tuple<double, double>(0, 0);
                try
                {
                    currentLocation = await GeoLocationService.GetCurrentLocation();
                }
                catch (GeolocationException)
                {
                    await _pageService.DisplayAlert("GPS Apagado", "No se pueden obtener coordenadas.", "Continuar");
                }
                if (Case.Client.Coordinates.Latitude == 0 && Case.Client.Coordinates.Longitude == 0 && currentLocation.Item1 != 0 && currentLocation.Item2 != 0)
                {
                    Case.Client.Coordinates = new CoordViewModel(new Coord { Latitude = currentLocation.Item1, Longitude = currentLocation.Item2 });
                    DoesHaveCoords = true;
                }
                bool error = false;
                //try
                //{
                //    float distance = (float)GeoLocationService.GetDistance(Case.Client.Coordinates.Latitude, Case.Client.Coordinates.Longitude, currentLocation.Item1, currentLocation.Item2, meters);
                //    if (distance > meters)
                //    {
                //        await _pageService.DisplayAlert("Error de ubicación", String.Format("Debe estar a menos de {0} metros del sitio del cliente para abrir una boleta. Distancia actual: {1:0} metros.", meters, distance), "Ok");
                //        IsBusy = false;
                //        return;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    error = true;
                //    await _pageService.DisplayAlert("Error de calculo", ex.Message + ". Aún con este error, te crearemos la boleta de servicio, pero las coordenadas no se actualizaran", "Ok");
                //}
                #endregion
                Incident acase = Case.ToModel();
                #region Create Service Ticket with Client Coordinates
                ServiceTicket created = null;
                try
                {
                    if (CrossConnectivity.Current.IsConnected)
                    {
                    try
                    {
                        created = await CRMConnector.CreateNewServiceTicket(acase, error);
                }
                        catch (Exception ex)
                {
                    await _pageService.DisplayAlert("Error", ex.Message, "Ok");
                }
            }
            else
                        created = await CRMConnector.CreateNewServiceTicketOffline(acase, error);
            }
                catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
            }
            if (created != null)
                {
                    NotificationService.DisplayMessage("Creada", "Creación finalizada con éxito");
                    Case.ServiceTickets.Add(new ServiceTicketViewModel(created));
                    ServiceTicketsHeight = Case.ServiceTickets.Count * 50;
                }
                else
                    await _pageService.DisplayAlert("Error", "No se creó la boleta por un error de ejecución", "Ok");
                #endregion
                //ServiceTicket created = await RestService.CreateNewServiceTicket(acase);               
                IsBusy = false;
            }
        }

        /// <summary>
        /// Elimina una boleta de servicio seleccionada.
        /// </summary>
        /// <param name="stToDelete">Instancia de boleta de servicio a eliminar.</param>
        /// <returns>Void</returns>
        private async Task DeleteServiceTicket(ServiceTicketViewModel stToDelete)
        {
            IsBusy = true;
            try
            {
                await RestService.DeleteServiceTicket(stToDelete.ToModel());
                Case.ServiceTickets.Remove(stToDelete);
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error", ex.Message, "Ok");
            }
            IsBusy = false;
        }

        /// <summary>
        /// Abre App Waze con las coordenadas del cliente.
        /// </summary>
        private void OpenWaze()
        {
            IsBusy = true;
            Device.OpenUri(new Uri(String.Format("https://waze.com/ul?ll={0},{1}&navigate=yes", Case.Client.Coordinates.Latitude, Case.Client.Coordinates.Longitude)));
            IsBusy = false;
        }

        /// <summary>
        /// Abre una boleta de servicio para su realización o revisión.
        /// </summary>
        /// <param name="stvm">Boleta de servicio que se abrirá</param>
        /// <returns>Void</returns>
        private async Task OpenServiceTicket(ServiceTicketViewModel stvm)
        {
            if (SelectedServiceTicket == null)
                return;
            if (SelectedServiceTicket.Finished.Equals(default(DateTime)))
            {
                ObservableCollection<TechnicianViewModel> AvailableTechnicians = await CRMConnector.GetTechniciansViewModel();
                for (int i = 1; i < 5; i++)
                    if (SelectedServiceTicket.Technicians[i] != null)
                        SelectedServiceTicket.Technicians[i] = AvailableTechnicians.Where((t) => t.InternalId == SelectedServiceTicket.Technicians[i].InternalId).FirstOrDefault();
                await _pageService.PushAsync(new EditableServiceTicketPage(ref RefCase, Case.ServiceTickets.IndexOf(SelectedServiceTicket), AvailableTechnicians));
            }
            else
                await _pageService.PushAsync(new ServiceTicketPage(ref RefCase, Case.ServiceTickets.IndexOf(SelectedServiceTicket)));
            SelectedServiceTicket = null;
        }
        #endregion
    }
}
