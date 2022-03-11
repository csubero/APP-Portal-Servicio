using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class EditableServiceTicketViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _Case;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private MaterialYRepuestoViewModel _SelectedProduct;
        private TimeSpan _EllapsedTime;
        private ObservableCollection<NoteViewModel> _PhotosTaken;
        private ObservableCollection<TechnicianViewModel> _AvailableTechnicians;
        private NoteViewModel _SelectedPhoto;
        private int _PhotoListHeight;
        private int _ProductListHeight;
        private bool _IsBusy;
        private bool _IsRefreshing;
        private bool _IsRxEnabled;
        private bool _IsLoadingPhotos;
        private readonly IPageService _pageService;

        public IncidentViewModel Case
        {
            get { return _Case; }
            set { SetValue(ref _Case, value); }
        }
        public ref IncidentViewModel RefCase
        {
            get { return ref _Case; }
        }
        public ServiceTicketViewModel SelectedServiceTicket
        {
            get { return _SelectedServiceTicket; }
            set
            {
                SetValue(ref _SelectedServiceTicket, value);
                ProductListHeight = SelectedServiceTicket.ProductsUsed.Count * 45;
            }
        }
        public MaterialYRepuestoViewModel SelectedProduct
        {
            get { return _SelectedProduct; }
            set { SetValue(ref _SelectedProduct, value); }
        }
        public ref ServiceTicketViewModel RefSelectedServiceTicket
        {
            get { return ref _SelectedServiceTicket; }
        }
        public TimeSpan EllapsedTime
        {
            get { return _EllapsedTime; }
            set { SetValue(ref _EllapsedTime, value); }
        }
        public ObservableCollection<NoteViewModel> PhotosTaken
        {
            get { return _PhotosTaken; }
            set { SetValue(ref _PhotosTaken, value); }
        }
        public ObservableCollection<TechnicianViewModel> AvailableTechnicians
        {
            get { return _AvailableTechnicians; }
            set { SetValue(ref _AvailableTechnicians, value); }
        }
        public NoteViewModel SelectedPhoto
        {
            get { return _SelectedPhoto; }
            set { SetValue(ref _SelectedPhoto, value); }
        }
        public int ProductListHeight
        {
            get { return _ProductListHeight; }
            set { SetValue(ref _ProductListHeight, value); }
        }
        public int PhotoListHeight
        {
            get { return _PhotoListHeight; }
            set { SetValue(ref _PhotoListHeight, value); }
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
        public bool IsLoadingPhotos
        {
            get { return _IsLoadingPhotos; }
            private set { SetValue(ref _IsLoadingPhotos, value); }
        }
        public bool IsRxEnabled
        {
            get { return _IsRxEnabled; }
            private set { SetValue(ref _IsRxEnabled, value); }
        }

        public ICommand DeleteProductCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand AddTechnicalServiceCostCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand OpenPhotoCommand { get; private set; }
        public ICommand DeletePhotoCommand { get; private set; }
        public ICommand SaveChangesCommand { get; private set; }
        public ICommand DoFeedbackCommand { get; private set; }
        public ICommand OpenRXChecklistCommand { get; private set; }
        public ICommand ReloadServiceTicketCommand { get; private set; }
        #endregion

        #region Constructors
        public EditableServiceTicketViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST, ObservableCollection<TechnicianViewModel> techs)
        {
            _pageService = pageService;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            if (!string.IsNullOrEmpty(SelectedServiceTicket.Type.Name) && SelectedServiceTicket.Type.Name.Equals("Rayos X"))
                IsRxEnabled = true;
            PhotosTaken = new ObservableCollection<NoteViewModel>();
            AvailableTechnicians = techs;
            EllapsedTime = new TimeSpan();
            Timer ellapsedTimer = new Timer();
            ellapsedTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            ellapsedTimer.Interval = 1000;
            ellapsedTimer.Enabled = true;
            PhotoListHeight = 0;

            AddProductCommand = new Command(async () => await AddProduct());
            SaveChangesCommand = new Command(async () => await SaveChanges());
            OpenRXChecklistCommand = new Command(async () => await OpenRXChecklist());
            TakePhotoCommand = new Command(async () => await TakePhoto());
            OpenPhotoCommand = new Command(async () => await OpenPhoto());
            DeletePhotoCommand = new Command<NoteViewModel>(async (photo) => await DeletePhoto(photo));
            DeleteProductCommand = new Command(async () => await DeleteProduct());
            DoFeedbackCommand = new Command(async () => await DoFeedbackIfNecessary());
            AddTechnicalServiceCostCommand = new Command(async () => await OpenTechnicalServiceCostPage());
            ReloadServiceTicketCommand = new Command(async () => await ReloadServiceTicket());
            LoadPhotos().ConfigureAwait(false);
        }
        #endregion

        #region Events
        /// <summary>
        /// Llama a funcion de guardar cambios y luego, determina si es necesario realizar opinion de usuario o finalizacion de boleta. Una vez determinado, abre dicha actividad.
        /// </summary>
        /// <returns></returns>
        private async Task DoFeedbackIfNecessary()
        {
            if (IsBusy)
                return;
            bool IsSaved = await SaveChanges();
            IsBusy = true;
            if (IsSaved)
                //if (!SelectedServiceTicket.FeedbackSubmitted)
                //    await _pageService.PopUpPushAsync(new FeedbackPopUpPage(ref RefCase, Case.ServiceTickets.IndexOf(SelectedServiceTicket), PhotosTaken.ToList()));
                //else
                await _pageService.PushAsync(new SignReportPage(ref RefCase, RefCase.ServiceTickets.IndexOf(SelectedServiceTicket), PhotosTaken.ToList()));
            IsBusy = false;
        }

        /// <summary>
        /// Carga las fotos asociadas a una boleta de servicio.
        /// </summary>
        /// <returns>Void</returns>
        private async Task LoadPhotos()
        {
            IsLoadingPhotos = true;
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    PhotosTaken = await CRMConnector.GetPhotosOfServiceTicket(SelectedServiceTicket.ToModel());
                }
                catch (Exception)
                {
                    NotificationService.DisplayMessage("Fotos no disponibles", "Un error impide la obtención de las fotos adjuntas a esta boleta");
                }
            }
            else
            {
                try
                {
                    NotificationService.DisplayMessage("Sin internet", "Cargando localmente.");
                    PhotosTaken = await CRMConnector.GetPhotosOfServiceTicketOffline(SelectedServiceTicket.ToModel());
                }
                catch (Exception ex)
                {
                    await _pageService.DisplayAlert("Error con internet", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            PhotoListHeight = PhotosTaken.Count * 45;
            IsLoadingPhotos = false;
        }

        /// <summary>
        /// Elimina una foto de una boleta de servicio. Esto se refleja instantaneamente en el servidor si hay internet, o se guarda en base de datos local para que sea efectuado al tener internet nuevamente.
        /// </summary>
        /// <param name="photo">Foto a eliminar</param>
        /// <returns>Void</returns>
        private async Task DeletePhoto(NoteViewModel photo)
        {

            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                    await CRMConnector.DeletePhotoTaken(photo.ToModel());
                else
                    await CRMConnector.DeletePhotoTakenOffline(photo.ToModel());
                PhotosTaken.Remove(photo);
                PhotoListHeight = PhotosTaken.Count * 45;
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert("Error", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
            }
            IsBusy = false;
        }

        /// <summary>
        /// Guarda los cambios de la boleta en el CRM o en la base de datos local para su posterior sincronización.
        /// </summary>
        /// <returns>Void</returns>
        private async Task<bool> SaveChanges()
        {
            if (IsBusy)
                return false;
            IsBusy = true;
            bool IsSaved = false;
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    IsSaved = await CRMConnector.SaveServiceTicket(SelectedServiceTicket.ToModel());
                    if (IsSaved)
                        NotificationService.DisplayMessage("Guardado", "Los cambios se guardaron satisfactoriamente");
                    else
                        await _pageService.DisplayAlert("Error", "Hubo un error al intentar guardar la información en el servidor.", "Ok");
                }
                catch (Exception ex)
                {
                    IsSaved = true;
                    if (await _pageService.DisplayAlert("Guardado", "Se ha guardado con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                        await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            else
            {
                try
                {
                    IsSaved = await CRMConnector.SaveServiceTicketOffline(SelectedServiceTicket.ToModel());
                    NotificationService.DisplayMessage("Guardado localmente", "Los cambios se sincronizaran al recuperar la conexión.");
                }
                catch (Exception ex)
                {
                    await _pageService.DisplayAlert("Error offline", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            IsBusy = false;
            return IsSaved;
        }

        /// <summary>
        /// Abre la ventana de mantenimiento de rayos X. Threadsafe.
        /// </summary>
        /// <returns>Void</returns>
        private async Task OpenRXChecklist() =>
            await _pageService.PushAsync(new XRayChecklistMPage(ref RefSelectedServiceTicket));

        /// <summary>
        /// Abre la ventana para agregar cotizaciones de mano de obra. ThreadSafe.
        /// </summary>
        /// <returns>Void</returns>
        private async Task OpenTechnicalServiceCostPage() =>
            await _pageService.PopUpPushAsync(new AddTechnicalServiceCostPopUpPage(ref RefCase, RefCase.ServiceTickets.IndexOf(SelectedServiceTicket)));

        /// <summary>
        /// Abre ventana de agregar un material y repuesto. ThreadSafe.
        /// </summary>
        /// <returns>Void</returns>
        private async Task AddProduct() =>
            await _pageService.PopUpPushAsync(new AddProductPopUpPage(ref RefCase, Case.ServiceTickets.IndexOf(SelectedServiceTicket)));

        /// <summary>
        /// Elimina un materialYRepuesto seleccionado.
        /// </summary>
        /// <returns>Void</returns>
        private async Task DeleteProduct()
        {
            if (IsBusy || SelectedProduct == null)
                return;
            IsBusy = true;
            if (await _pageService.DisplayAlert("Desea remover producto?", "Quitar este producto de esta boleta?", "Sí", "No"))
            {
                try
                {
                    if (CrossConnectivity.Current.IsConnected && !SelectedProduct.InternalId.Equals(default(Guid)))
                        await CRMConnector.DeleteMaterial(SelectedProduct.InternalId);
                    else
                        await CRMConnector.DeleteMaterialOffline(SelectedProduct.ToModel());
                    SelectedServiceTicket.ProductsUsed.Remove(SelectedProduct);
                }
                catch (Exception ex)
                {
                    await _pageService.DisplayAlert("Error al eliminar producto", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            SelectedProduct = null;
            IsBusy = false;
        }

        /// <summary>
        /// Abre la foto obtenida del CRM. Genera un archivo en el telefono correspondiente a la foto. Ocupa permiso de almacenamiento.
        /// </summary>
        /// <returns>Void</returns>
        private async Task OpenPhoto()
        {
            if (IsBusy || SelectedPhoto == null)
                return;
            IsBusy = true;
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                if (results.ContainsKey(Permission.Storage))
                    status = results[Permission.Storage];
            }
            if (status == PermissionStatus.Granted)
            {
                try //Save to storage
                {
                    byte[] decodedFile = Convert.FromBase64String(SelectedPhoto.Content);
                    MemoryStream memstream = new MemoryStream(decodedFile);
                    DependencyService.Get<ISave>().SaveTextAsync(SelectedPhoto.Filename, "image/jpeg", memstream, true);
                }
                catch (Exception)
                {
                    throw new FileNotFoundException(String.Format("No se pudo acceder al archivo {0}", SelectedPhoto.Filename));
                }
            }
            else
                await _pageService.DisplayAlert("Sin acceso a almacenamiento", "Debe conceder acceso al almacenamiento para acceder a este archivo", "Ok");
            SelectedPhoto = null;
            IsBusy = false;
        }

        /// <summary>
        /// Toma una foto a través de la cámara, y la guarda localmente. Posteriormente, la envia al Servidor si tiene conexión, o la guarda en la base de datos local si no es así.
        /// </summary>
        /// <returns>Void</returns>
        private async Task TakePhoto()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            #region Ask For Permission
            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }
            #endregion
            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    DefaultCamera = CameraDevice.Rear,
                    PhotoSize = PhotoSize.Medium,
                    CompressionQuality = 30
                });
                if (file == null) // Action is cancelled
                {
                    IsBusy = false;
                    return;
                }
                Stream bytes = file.GetStreamWithImageRotatedForExternalStorage();
                MemoryStream ms = new MemoryStream();
                bytes.CopyTo(ms);
                Note photo = new Note
                {
                    ObjectId = SelectedServiceTicket.InternalId,
                    Filename = String.Format("AdjuntoDeBoleta{0}({1}).jpeg", SelectedServiceTicket.TicketNumber, PhotosTaken.Count + 1),
                    Title = String.Format("AdjuntoDeBoleta{0}({1}).jpeg", SelectedServiceTicket.TicketNumber, PhotosTaken.Count + 1),
                    Content = Convert.ToBase64String(ms.ToArray()),
                    ServiceTicket = SelectedServiceTicket.ToModel(),
                    ServiceTicketId = SelectedServiceTicket.SQLiteRecordId,
                    Mime = "image/jpeg",
                };
                try
                {
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        photo.InternalId = await CRMConnector.AddPhoto(photo);
                        NotificationService.DisplayMessage("Foto subida", "Los foto se subió satisfactoriamente.");
                    }
                    else
                    {
                        NotificationService.DisplayMessage("Sin internet", "Guardando localmente");
                        await CRMConnector.AddPhotoOffline(photo);
                    }
                    PhotosTaken.Add(new NoteViewModel(photo));
                    PhotoListHeight = PhotosTaken.Count * 45;
                }
                catch (Exception)
                {
                    await _pageService.DisplayAlert("Error", "Sucedió un error al subir el adjunto, intentelo de nuevo.", "Ok");
                }
            }
            else
            {
                await _pageService.DisplayAlert("Permiso Denegado", "No es posible tomar fotos", "OK");
                CrossPermissions.Current.OpenAppSettings();
            }
            IsBusy = false;
        }

        /// <summary>
        /// Recarga la boleta de servicio con información actualizada si hay internet o carga lo más reciente de la base de datos local.
        /// </summary>
        /// <returns>Void</returns>
        private async Task ReloadServiceTicket()
        {
            if (IsRefreshing)
                return;
            IsRefreshing = true;
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                    Case = new IncidentViewModel(await CRMConnector.GetIncident(Case.InternalId));
                else
                    Case = new IncidentViewModel(await CRMConnector.GetLocalIncident(Case.InternalId));
                SelectedServiceTicket = Case.ServiceTickets.Where(st => st.SQLiteRecordId == SelectedServiceTicket.SQLiteRecordId).FirstOrDefault();
            }catch(Exception ex)
            {
                await _pageService.DisplayAlert(Config.MSG_ERR_TITLE_GENERIC, ex.Message, "Ok");
            }
            IsRefreshing = false;
        }

        /// <summary>
        /// Evento que actualiza el tiempo transcurrido de la boleta.
        /// </summary>
        /// <param name="source">Origen del evento.</param>
        /// <param name="e">Argumentos de cambio del tiempo.</param>
        private void OnTimedEvent(object source, ElapsedEventArgs e) =>
            EllapsedTime = DateTime.Now - SelectedServiceTicket.Started;
        #endregion
    }
}
