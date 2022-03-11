using Plugin.Connectivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalAPI.Contracts;
using PortalServicio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class ProgramCaseViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _SelectedIncident;
        private ObservableCollection<TechnicianViewModel> _AvailableTechnicians;
        private ObservableCollection<NoteViewModel> _Notes;
        private NoteViewModel _SeletedNote;
        private Dictionary<string, Types.SPCINCIDENT_CONTROLOPTION> _Dic_ControlOptions;
        private Dictionary<string, Types.SPCINCIDENT_PAYMENTOPTION> _Dic_PaymentOptions;
        private int _NotesHeight;
        private bool _IsBusy;
        private bool _IsLoading;
        private bool _IsLoadingNotes;
        private readonly IPageService _pageService;

        public IncidentViewModel SelectedIncident
        {
            get { return _SelectedIncident; }
            set { SetValue(ref _SelectedIncident, value); }
        }
        public ObservableCollection<NoteViewModel> Notes
        {
            get { return _Notes; }
            private set { SetValue(ref _Notes, value); }
        }
        public NoteViewModel SelectedNote
        {
            get { return _SeletedNote; }
            set { SetValue(ref _SeletedNote, value); }
        }
        public ObservableCollection<TechnicianViewModel> AvailableTechnicians
        {
            get { return _AvailableTechnicians; }
            private set { SetValue(ref _AvailableTechnicians, value); }
        }
        public Dictionary<string, Types.SPCINCIDENT_CONTROLOPTION> Dic_ControlOptions
        {
            get { return _Dic_ControlOptions; }
            private set { SetValue(ref _Dic_ControlOptions, value); }
        }
        public List<KeyValuePair<string, Types.SPCINCIDENT_CONTROLOPTION>> Dic_ControlOptionsList
        {
            get { return Dic_ControlOptions.ToList(); }
        }
        public Dictionary<string, Types.SPCINCIDENT_PAYMENTOPTION> Dic_PaymentOptions
        {
            get { return _Dic_PaymentOptions; }
            private set { SetValue(ref _Dic_PaymentOptions, value); }
        }
        public List<KeyValuePair<string, Types.SPCINCIDENT_PAYMENTOPTION>> Dic_PaymentOptionsList
        {
            get { return Dic_PaymentOptions.ToList(); }
        }
        public int NotesHeight
        {
            get { return _NotesHeight; }
            private set { SetValue(ref _NotesHeight, value); }
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
        public bool IsLoadingNotes
        {
            get { return _IsLoadingNotes; }
            set { SetValue(ref _IsLoadingNotes, value); }
        }

        public ICommand OpenNoteCommand { get; private set; }
        public ICommand SaveChangesCommand { get; private set; }
        public ICommand LoadNotesCommand { get; private set; }
        #endregion

        #region Constructors
        public ProgramCaseViewModel(IPageService pageService, ref IncidentViewModel selected, IEnumerable<TechnicianViewModel> techs)
        {
            _pageService = pageService;
            SelectedIncident = selected;
            if (SelectedIncident.ProgrammedDate.Equals(default(DateTime)))
                SelectedIncident.ProgrammedDate = DateTime.Now;
            AvailableTechnicians = new ObservableCollection<TechnicianViewModel>(techs);
            Dic_ControlOptions = new Dictionary<string, Types.SPCINCIDENT_CONTROLOPTION>
            {
                { "Finalizado", Types.SPCINCIDENT_CONTROLOPTION.Finalizado },
                { "En Reproceso", Types.SPCINCIDENT_CONTROLOPTION.Reproceso },
                { "En Reprogramación", Types.SPCINCIDENT_CONTROLOPTION.Reprogramacion },
                { "Programado", Types.SPCINCIDENT_CONTROLOPTION.Programado },
                { "Esperando Revisión", Types.SPCINCIDENT_CONTROLOPTION.EsperandoRevision },
                { "Pendiente de cotizar", Types.SPCINCIDENT_CONTROLOPTION.PendienteCotizar },
            };
            Dic_PaymentOptions = new Dictionary<string, Types.SPCINCIDENT_PAYMENTOPTION>
            {
                { "Anticipo", Types.SPCINCIDENT_PAYMENTOPTION.Anticipo },
                { "Informativo", Types.SPCINCIDENT_PAYMENTOPTION.Informativo },
                { "Verificación", Types.SPCINCIDENT_PAYMENTOPTION.Verificacion },
                { "Garantía", Types.SPCINCIDENT_PAYMENTOPTION.Garantia },
                { "Retiro", Types.SPCINCIDENT_PAYMENTOPTION.Retiro },
                { "N/A", Types.SPCINCIDENT_PAYMENTOPTION.NA },
                { "Adelanto", Types.SPCINCIDENT_PAYMENTOPTION.Adelanto },
            };
            SaveChangesCommand = new Command(async ()=> await SaveChanges());
            LoadNotesCommand = new Command(async () => await LoadNotes());
            OpenNoteCommand = new Command(async () => await OpenNote());
            LoadNotesCommand?.Execute(null);
        }
        #endregion

        #region Events
        private async Task LoadNotes()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                NotificationService.DisplayMessage("Sin conexión a internet", "Debes tener conexión a internet obtener los adjuntos del caso.");
                return;
            }
            IsLoadingNotes = true;
            NotesHeight = 45;
            try
            {
                Notes = await CRMConnector.GetReportsFromIncident(SelectedIncident.InternalId);
                NotesHeight = Notes.Count * 45;
            }
            catch (Exception)
            {
                NotificationService.DisplayMessage("Adjuntos no disponibles", "Un error impide la obtención de los adjuntos a este caso.");
            }
            IsLoadingNotes = false;
        }

        private async Task SaveChanges()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await _pageService.DisplayAlert("Sin conexión a internet", "Debes tener conexión a internet para realizar esta operación.", "Ok");
                return;
            }
            if (IsBusy)
                return;
            IsBusy = true;
            if (await CRMConnector.UpdateIncidentProgrammingInformation(SelectedIncident.ToModel()))
                NotificationService.DisplayMessage("Guardado", "Los cambios se guardaron satisfactoriamente");
            else
                await _pageService.DisplayAlert("Error", "Sucedió un error al actualizar el caso. Intente de nuevo. Si el problema persiste contacte TI.", "Ok");
            IsBusy = false;
        }

        private async Task OpenNote()
        {
            if (IsBusy || _SeletedNote == null)
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
                    byte[] decodedFile = Convert.FromBase64String(SelectedNote.Content);
                    MemoryStream memstream = new MemoryStream(decodedFile);
                    DependencyService.Get<ISave>().SaveTextAsync(SelectedNote.Filename, "application/pdf", memstream, true);
                }
                catch (Exception)
                {
                    NotificationService.DisplayMessage("Error", String.Format("No se pudo acceder al archivo {0}", SelectedNote.Filename));
                }
            }
            else
                NotificationService.DisplayMessage("Sin acceso a almacenamiento", "Se requiere permisos para realizar la operación");
            _SeletedNote = null;
            IsBusy = false;
        }
        #endregion
    }
}
