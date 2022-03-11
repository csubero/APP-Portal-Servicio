using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Models;
using PortalServicio.Services;
using PortalServicio.Views;
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
    public class CDTTicketSummaryViewModel : BaseViewModel
    {
        #region Properties
        private CDTViewModel _CDT;
        private CDTTicketViewModel _SelectedCDTTicket;
        private TechnicianRegistryViewModel _SelectedTechnicianRegistry;
        private bool _IsBusy;
        private bool _IsRefreshing;
        private bool _IsLoadingPhotos;
        private int _CDTTicketsHeight;
        private TimeSpan _EllapsedTime;
        private readonly IPageService _pageService;
        private ObservableCollection<NoteViewModel> _PhotosTaken;
        private NoteViewModel _SelectedPhoto;
        private int _PhotoListHeight;

        public CDTViewModel CDT
        {
            get { return _CDT; }
            set
            {
                SetValue(ref _CDT, value);
                CDTTicketsHeight = CDT.CDTTickets.Count * 50;
            }
        }
        public ref CDTViewModel RefCDT
        {
            get { return ref _CDT; }
        }
        public CDTTicketViewModel SelectedCDTTicket
        {
            get { return _SelectedCDTTicket; }
            set { SetValue(ref _SelectedCDTTicket, value); }
        }
        public TechnicianRegistryViewModel SelectedTechnicianRegistry
        {
            get { return _SelectedTechnicianRegistry; }
            set { SetValue(ref _SelectedTechnicianRegistry, value); }
        }
        public NoteViewModel SelectedPhoto
        {
            get { return _SelectedPhoto; }
            set { SetValue(ref _SelectedPhoto, value); }
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
        public int CDTTicketsHeight
        {
            get { return _CDTTicketsHeight; }
            set { SetValue(ref _CDTTicketsHeight, value); }
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

        public int PhotoListHeight
        {
            get { return _PhotoListHeight; }
            set { SetValue(ref _PhotoListHeight, value); }
        }
        public ICommand SaveCommand { get; private set; }
        public ICommand FinishTicketCommand { get; private set; }
        public ICommand SignCDTTicketCommand { get; private set; }
        public ICommand ToggleCollapseTechnicianRegistryCommand { get; private set; }
        public ICommand ReloadCDTTicketCommand { get; private set; }
        public ICommand AddTechnicianRegistryCommand { get; private set; }
        public ICommand DeleteTechnicianRegistryCommand { get; private set; }
        public ICommand SaveChangesTechnicianRegistryCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand BackPressedCommand { get; private set; }
        public ICommand OpenPhotoCommand { get; private set; }

        private Timer _ellapsedTimer { set; get; }
        #endregion

        #region Constructors
        public CDTTicketSummaryViewModel(IPageService pageService, CDTViewModel cdt, int indexOfSelectedTicket)
        {
            _pageService = pageService;
            CDT = cdt;
            SelectedCDTTicket = cdt.CDTTickets[indexOfSelectedTicket];
            SaveCommand = new Command(async () => await Save());
            //FinishTicketCommand = new Command(async () => await CloseTicket());
            ToggleCollapseTechnicianRegistryCommand = new Command(ToggleCollapseTechnicianRegistry);
            ReloadCDTTicketCommand = new Command(async () => await ReloadCDTTicket());
            AddTechnicianRegistryCommand = new Command(async () => await AddTechnicianRegistry());
            DeleteTechnicianRegistryCommand = new Command<TechnicianRegistryViewModel>(async (TechnicianRegistryViewModel r) => await DeleteRegistry(r));
            SaveChangesTechnicianRegistryCommand = new Command<TechnicianRegistryViewModel>(async (TechnicianRegistryViewModel r) => await SaveRegistry(r));
            TakePhotoCommand = new Command(async () => await TakePhoto());
            OpenPhotoCommand = new Command(async () => await OpenPhoto());
            SignCDTTicketCommand = new Command(async () => await SignCDTTicket());
            BackPressedCommand = new Command(BackPressed);
            if (SelectedCDTTicket.IsOpen)
            {
                EllapsedTime = new TimeSpan();
                _ellapsedTimer = new Timer();
                _ellapsedTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                _ellapsedTimer.Interval = 1000;
                _ellapsedTimer.Enabled = true;
            }
            PhotosTaken = new ObservableCollection<NoteViewModel>();
            LoadPhotos().ConfigureAwait(false);
        }
        #endregion

        private async Task Save()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            await CRMConnector.SaveCDTTicket(SelectedCDTTicket.ToModel());
            IsBusy = false;
        }

        private void BackPressed()
        {
            if (_ellapsedTimer != null)
            {
                _ellapsedTimer.Enabled = false;
                _ellapsedTimer.Stop();
                _ellapsedTimer.Dispose();
            }
        }

        private async Task LoadPhotos()
        {
            IsLoadingPhotos = true;
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    PhotosTaken = await CRMConnector.GetPhotosOfCDTTicket(SelectedCDTTicket.ToModel(), CDT.ToModel());
                }
                catch (Exception ex)
                {
                    NotificationService.DisplayMessage("Fotos no disponibles", "Un error impide la obtención de las fotos adjuntas a esta boleta");
                }
            }
            //else
            //{
            //    try
            //    {
            //        NotificationService.DisplayMessage("Sin internet", "Cargando localmente.");
            //        PhotosTaken = await CRMConnector.GetPhotosOfServiceTicketOffline(SelectedServiceTicket.ToModel());
            //    }
            //    catch (Exception ex)
            //    {
            //        await _pageService.DisplayAlert("Error con internet", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
            //    }
            //}
            PhotoListHeight = PhotosTaken.Count * 45;
            IsLoadingPhotos = false;
        }

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

        private async Task SignCDTTicket()
        {
            if (SelectedCDTTicket.TechniciansRegistered.Count == 0)
                await _pageService.DisplayAlert("Necesitas asignar técnicos", "Debes agregar al menos 1 técnico para cerrar la boleta", "Comprendo");
            else
                await _pageService.PopUpPushAsync(new CDTTicketSignPopUpPage(ref RefCDT, CDT.CDTTickets.IndexOf(SelectedCDTTicket), EllapsedTime)/*ref RefCDT, CDT.CDTTickets.IndexOf(SelectedCDTTicket)*/);
        }

        //private async Task CloseTicket()
        //{
        //    if (IsBusy)
        //        return;
        //    if (SelectedCDTTicket.TechniciansRegistered.Count == 0)
        //    {
        //        await _pageService.DisplayAlert("Necesitas asignar técnicos", "Debes agregar al menos 1 técnico para cerrar la boleta", "Comprendo");
        //        return;
        //    }
        //    IsBusy = true;
        //    SelectedCDTTicket.Finished = SelectedCDTTicket.Started + EllapsedTime;
        //    foreach (TechnicianRegistryViewModel tech in SelectedCDTTicket.TechniciansRegistered)
        //    {
        //        if (!tech.IsDatetimeSet)
        //            tech.Finished = SelectedCDTTicket.Finished;
        //        List<DateTime> holydays = await CRMConnector.GetAllHolydays();
        //        double[] hours = Configuration.Config.CalculateHours(tech.Started, tech.Finished, holydays);
        //        tech.HoursNormal = hours[0];
        //        tech.HoursNormalNight = hours[2];
        //        tech.HoursDaytimeExtra = hours[1];
        //        tech.HoursNightExtra = hours[3];
        //        tech.HoursHolydayDaytime = hours[4];
        //        tech.HoursHolydayNight = hours[5];
        //        tech.HoursOffdayDaytime = hours[6];
        //        tech.HoursOffdayNight = hours[7];
        //        tech.HoursOffdayDaytimeExtra = hours[8];
        //        tech.HoursOffdayNightExtra = hours[9];
        //    }
        //    #region Request Permissions
        //    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
        //    if (status != PermissionStatus.Granted)
        //    {
        //        var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
        //        if (results.ContainsKey(Permission.Storage))
        //            status = results[Permission.Storage];
        //    }
        //    #endregion
        //    if (status == PermissionStatus.Granted)
        //    {
        //        await PDFServices.CreateCDTTicketReport(SelectedCDTTicket.ToModel(), CDT.ToModel(),);
        //        byte[] doc = DependencyService.Get<ISave>().GetDocumentBytesAsync("BoletaDeCDT.pdf");
        //        Note STDoc = new Note
        //        {
        //            Filename = "ReporteDeCDT" + CDT.Number + ".pdf",
        //            ObjectId = CDT.InternalId,
        //            Mime = "application/pdf",
        //            Content = Convert.ToBase64String(doc),
        //            CDT = CDT.ToModel(),
        //            CDTId = CDT.SQLiteRecordId
        //        };
        //        if (!STDoc.ObjectId.Equals(default(Guid)))
        //        {
        //            try
        //            {
        //                if (!await CRMConnector.AddReportToCDT(STDoc))
        //                {
        //                    await _pageService.DisplayAlert("Error al subir boleta", "No se ha podido subir la boleta al servidor. Sin embargo, se creó una copia local en la carpeta recibos", "Ok");
        //                    IsBusy = false;
        //                    return;
        //                }
        //                else
        //                    NotificationService.DisplayMessage("Subida Correcta", "Se ha subido una boleta de servicio.");
        //            }
        //            catch (Exception ex)
        //            {
        //                if (await _pageService.DisplayAlert("Envío fallido", "Se ha enviado el reporte con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
        //                    await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
        //            }
        //        }
        //        else
        //        {
        //            NotificationService.DisplayMessage("Error", "Sucedio un error al subir el reporte");
        //            //NotificationService.DisplayMessage("Guardado localmente", "Se sincronizará cuando se recupere la conexión.");
        //            //await CRMConnector.AddServiceTicketSignedOffline(STDoc);
        //        }
        //        //await CRMConnector.FinishCDTTicket(SelectedCDTTicket.ToModel());
        //        _ellapsedTimer.Enabled = false;
        //        await _pageService.PopAsync();
        //    }
        //    else
        //        NotificationService.DisplayMessage("Sin permisos", "Se requiere permitir acceso a memoria del dispositivo para crear el pdf.");
        //    IsBusy = false;
        //}

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
                    ObjectId = SelectedCDTTicket.InternalId,
                    Filename = String.Format("AdjuntoDe{0}({1}).jpeg", SelectedCDTTicket.Number, PhotosTaken.Count + 1),
                    Title = SelectedCDTTicket.Number, /*String.Format("AdjuntoDe{0}({1}).jpeg", SelectedCDTTicket.Number, PhotosTaken.Count + 1),*/
                    Content = Convert.ToBase64String(ms.ToArray()),
                    CDT = CDT.ToModel(),
                    CDTId = CDT.SQLiteRecordId,
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

        private async Task SaveRegistry(TechnicianRegistryViewModel registry)
        {
            if (IsBusy)
                return;
            IsBusy = true;
            registry.IsDatetimeSet = true;
            registry.Finished = new DateTime(SelectedCDTTicket.Started.Year, SelectedCDTTicket.Started.Month, SelectedCDTTicket.Started.Day + ((registry.Started.TimeOfDay > registry.Finished.TimeOfDay) ? 1 : 0)).Add(registry.Finished.TimeOfDay);
            await CRMConnector.SaveChangesTechnicianRegistry(registry.ToModel());
            IsBusy = false;
        }

        private async Task DeleteRegistry(TechnicianRegistryViewModel registry)
        {
            IsBusy = true;
            await CRMConnector.DeleteTechnicianRegistry(registry.InternalId);
            SelectedCDTTicket.TechniciansRegistered.Remove(registry);
            IsBusy = false;
        }

        private async Task ReloadCDTTicket()
        {
            IsRefreshing = true;
            CDT = new CDTViewModel(await CRMConnector.GetCDT(CDT.InternalId));
            SelectedCDTTicket = CDT.CDTTickets.Where(ticket => ticket.SQLiteRecordId == SelectedCDTTicket.SQLiteRecordId).FirstOrDefault();
            if (SelectedCDTTicket == null)
            {
                if (_ellapsedTimer != null)
                    _ellapsedTimer.Enabled = false;
                await _pageService.PopAsync();
            }
            IsRefreshing = false;
        }

        private async Task AddTechnicianRegistry() =>
            await _pageService.PopUpPushAsync(new AddTechnicianRegistryPopUpPage(ref RefCDT, CDT.CDTTickets.IndexOf(SelectedCDTTicket)));

        private void ToggleCollapseTechnicianRegistry()
        {
            if (SelectedTechnicianRegistry != null)
            {
                SelectedTechnicianRegistry.IsExtended = !SelectedTechnicianRegistry.IsExtended;
                SelectedTechnicianRegistry = null;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e) =>
           EllapsedTime = DateTime.Now - SelectedCDTTicket.Started;
    }
}
