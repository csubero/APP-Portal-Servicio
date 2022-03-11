using Plugin.Connectivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalServicio.Configuration;
using PortalServicio.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class ReadOnlyServiceTicketViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _Case;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private ObservableCollection<NoteViewModel> _PhotosTaken;
        private NoteViewModel _SelectedPhoto;
        private int _PhotoListHeight;
        private int _ProductListHeight;
        private bool _IsBusy;
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
                ProductListHeight = SelectedServiceTicket.ProductsUsed.Count * 40;
            }
        }
        public ObservableCollection<NoteViewModel> PhotosTaken
        {
            get { return _PhotosTaken; }
            set { SetValue(ref _PhotosTaken, value); }
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

        public ICommand OpenPhotoCommand { get; private set; }
        public ICommand ReloadServiceTicketCommand { get; private set; }
        #endregion

        #region Constructors
        public ReadOnlyServiceTicketViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST)
        {
            _pageService = pageService;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            PhotosTaken = new ObservableCollection<NoteViewModel>();
            PhotoListHeight = 0;

            OpenPhotoCommand = new Command(async () => await OpenPhoto());
            ReloadServiceTicketCommand = new Command(async () => await ReloadServiceTicket());
            LoadPhotos().ConfigureAwait(false);
        }
        #endregion

        #region Events
        private async Task LoadPhotos()
        {
            IsBusy = true;
            PhotosTaken = await CRMConnector.GetPhotosOfServiceTicket(SelectedServiceTicket.ToModel());
            PhotoListHeight = PhotosTaken.Count * 45;
            IsBusy = false;
        }

        public async Task OpenPhoto()
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

        private async Task ReloadServiceTicket()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                    Case = new IncidentViewModel(await CRMConnector.GetIncident(Case.InternalId));
                else
                    Case = new IncidentViewModel(await CRMConnector.GetLocalIncident(Case.InternalId));
                SelectedServiceTicket = Case.ServiceTickets.Where(st => st.SQLiteRecordId == SelectedServiceTicket.SQLiteRecordId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _pageService.DisplayAlert(Config.MSG_ERR_TITLE_GENERIC, ex.Message, "Ok");
            }
            IsBusy = false;
        }
        #endregion
    }
}
