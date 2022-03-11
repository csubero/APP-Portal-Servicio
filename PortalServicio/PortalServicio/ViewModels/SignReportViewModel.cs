using Plugin.Connectivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using PortalAPI.Contracts;
using PortalServicio.Configuration;
using PortalServicio.Models;
using PortalServicio.Services;
using SignaturePad.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    /// <summary>
    /// ViewModel que permite firmar la boleta de servicio para generar un PDF de reporte de servicio y subirlo al CRM.
    /// </summary>
    public class SignReportViewModel : BaseViewModel
    {
        #region Properties
        private string _AttendeeName;
        private string _AttendeeId;
        private IncidentViewModel _Case;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private bool _AttendeeAgrees;
        private bool _IsBusy;
        private readonly IPageService _pageService;
        private SignaturePadView _signaturePad;
        private readonly List<NoteViewModel> _Photos;

        #region Test
        private bool _IsBelow4;

        public double Rating1
        {
            get { return Case.FeedbackAnswer1; }
            set
            {
                Case.FeedbackAnswer1 = value;
                IsBelow4 = ((Case.FeedbackAnswer1 + Case.FeedbackAnswer2) / 2 < 4);
            }
        }
        public double Rating2
        {
            get { return Case.FeedbackAnswer2; }
            set
            {
                Case.FeedbackAnswer2 = value;
                IsBelow4 = ((Case.FeedbackAnswer1 + Case.FeedbackAnswer2) / 2 < 4);
            }
        }
        public string Feedback
        {
            get { return Case.ClientFeedback; }
            set { Case.ClientFeedback = value; }
        }
        public bool IsBelow4
        {
            get { return _IsBelow4; }
            set { SetValue(ref _IsBelow4, value); }
        }

        public ICommand SendFeedbackCommand { get; private set; }
        #endregion

        public string AttendeeName
        {
            get { return _AttendeeName; }
            set { SetValue(ref _AttendeeName, value); }
        }
        public string AttendeeEmail
        {
            get { return SelectedServiceTicket.Email; }
            set { SelectedServiceTicket.Email = value; }
        }
        public string AttendeeId
        {
            get { return _AttendeeId; }
            set { SetValue(ref _AttendeeId, value); }
        }
        public bool AttendeeAgrees
        {
            get { return _AttendeeAgrees; }
            set { SetValue(ref _AttendeeAgrees, value); }
        }
        public IncidentViewModel Case
        {
            get { return _Case; }
            set { SetValue(ref _Case, value); }
        }
        public ServiceTicketViewModel SelectedServiceTicket
        {
            get { return _SelectedServiceTicket; }
            set { SetValue(ref _SelectedServiceTicket, value); }
        }
        public ref ServiceTicketViewModel RefSelectedServiceTicket {
            get { return ref _SelectedServiceTicket; }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }

        public ICommand SignReportCommand { get; private set; }
        public ICommand CreateReportCommand { get; private set; }
        #endregion

        #region Constructors
        public SignReportViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST, ref SignaturePadView signpad, List<NoteViewModel> Photos)
        {
            _pageService = pageService;
            _signaturePad = signpad;
            _Photos = Photos;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            Case.FeedbackAnswer1 = 3;
            Case.FeedbackAnswer2 = 3;
            SendFeedbackCommand = new Command(async () => await SendFeedback());
            SignReportCommand = new Command(async () => await SignReport());
            CreateReportCommand = new Command(() => CreatePreview().ConfigureAwait(false));
            CreatePreview().ConfigureAwait(false);
        }
        #endregion

        #region Events
        /// <summary>
        /// Envía retroalimentación de cliente al servidor o la guarda localmente para ser enviada cuando se recupere la conexión a internet.
        /// </summary>
        /// <returns>Void</returns>
        private async Task<bool> SendFeedback()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    if (!await CRMConnector.SendReview(Case.ToModel(), SelectedServiceTicket.ToModel()))
                    {
                        IsBusy = false;
                        await _pageService.DisplayAlert("Error", "La comunicación con el servidor se interrumpió. Inténtelo de nuevo.", "Ok");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    if (await _pageService.DisplayAlert("Enviado", "Se ha guardado con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                        await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                    return false;
                }
            }
            else
                await CRMConnector.SendReviewOffline(Case.ToModel(), SelectedServiceTicket.ToModel());
            SelectedServiceTicket.FeedbackSubmitted = true;
            return true;
        }

        /// <summary>
        /// Firma el reporte de servicio final, cierra la boleta y regresa al listado de boletas de servicio.
        /// </summary>
        /// <returns>Void</returns>
        private async Task SignReport()
        {
            if (IsBusy)
                return;
            #region Validation of user inputs
            if (string.IsNullOrEmpty(AttendeeName) || string.IsNullOrEmpty(AttendeeId))
            {
                await _pageService.DisplayAlert("Información Incompleta", "Asegúrese de ingresar nombre e identificación.", "Ok");
                return;
            }
            #endregion
            IsBusy = true;
            if(!await SendFeedback())
            {
                IsBusy = false;
                return;
            }
            ProgressBasicPopUpViewModel vm = new ProgressBasicPopUpViewModel(new PageService(), "Cerrando boleta", "Obteniendo código", 0, 2);
            await _pageService.PopUpPushAsync(new ProgressBasicPopUpPage(ref vm));
            #region Obtain Sign
            ImageConstructionSettings settings = new ImageConstructionSettings
            {
                BackgroundColor = Color.White,
                StrokeColor = Color.Black,
                DesiredSizeOrScale = new SizeOrScale(150, 50, SizeOrScaleType.Size)
            };
            Stream stream = await _signaturePad.GetImageStreamAsync(SignatureImageFormat.Jpeg, settings);
            #endregion
            #region Obtain Code
            if (Case != null && Case.Client != null && Case.Client.Category != null && !string.IsNullOrEmpty(Case.Client.Category.Name))
                SelectedServiceTicket.Code = Config.CalculateCode(Case.Client.Category.Name, SelectedServiceTicket.MoneyCurrency.Name, SelectedServiceTicket.ToModel().Technicians, Config.CalculateWorkedTime(DateTime.Now - SelectedServiceTicket.Started, SelectedServiceTicket.HadLunch));
            #endregion
            #region Create & Upload Service Ticket Report
            SelectedServiceTicket.Finished = DateTime.Now;
            //SelectedServiceTicket.Finished = new DateTime(2018, 11, 15, 17, 0, 0); //Si necesita cerrar una boleta a una fecha especifca utilice este linea y comente la de arriba.
            bool connected = CrossConnectivity.Current.IsConnected;
            vm.ProgressUp("Generando reporte de servicio final");
            List<Note> photosmodel = new List<Note>();
            if (_Photos != null)
                foreach (NoteViewModel photo in _Photos)
                    photosmodel.Add(photo.ToModel());
            switch (Case.Client.ReportType)
            {
                case Types.SPCCLIENT_REPORTTYPEOPTION.ContableSimple:
                    await PDFServices.CreateFinancialServiceTicketReport(SelectedServiceTicket.ToModel(), Case.ToModel(), photosmodel, preview: false, sign: stream, identification: AttendeeId, name: AttendeeName, agree: AttendeeAgrees);
                    break;
                case Types.SPCCLIENT_REPORTTYPEOPTION.Normal:
                default:
                    await PDFServices.CreateServiceTicketReport(SelectedServiceTicket.ToModel(), Case.ToModel(), photosmodel, preview: false, sign: stream, identification: AttendeeId, name: AttendeeName);
                    break;
            }
            byte[] doc = DependencyService.Get<ISave>().GetDocumentBytesAsync("BoletaDeServicio.pdf");
            Note STDoc = new Note
            {
                Filename = "ReporteDeServicio" + SelectedServiceTicket.TicketNumber + ".pdf",
                ObjectId = Case.InternalId,
                Mime = "application/pdf",
                Content = Convert.ToBase64String(doc),
                Incident = Case.ToModel(),
                IncidentId = Case.SQLiteRecordId
            };
            if (connected && !STDoc.ObjectId.Equals(default(Guid)))
            {
                try
                {
                    if (!await CRMConnector.AddReportToIncident(STDoc))
                    {
                        await _pageService.DisplayAlert("Error al subir boleta", "No se ha podido subir la boleta al servidor. Sin embargo, se creó una copia local en la carpeta recibos", "Ok");
                        IsBusy = false;
                        return;
                    }
                    else
                        NotificationService.DisplayMessage("Subida Correcta", "Se ha subido una boleta de servicio.");
                }
                catch (Exception ex)
                {
                    if (await _pageService.DisplayAlert("Reporte enviada", "Se ha enviado el reporte con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                        await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            else
            {
                NotificationService.DisplayMessage("Guardado localmente", "Se sincronizará cuando se recupere la conexión.");
                await CRMConnector.AddServiceTicketSignedOffline(STDoc);
            }
            #endregion
            #region Create & Upload Legalization Report if available            
            if (SelectedServiceTicket.IsLegalizable())
            {
                vm.ProgressUp("Generando reporte de legalización");
                await PDFServices.CreateLegalizationReport(SelectedServiceTicket.ToModel(), Case.ToModel());
                byte[] content = DependencyService.Get<ISave>().GetDocumentBytesAsync("Legalizacion.pdf");
                Note note = new Note
                {
                    Filename = "LegalizacionBoleta" + SelectedServiceTicket.TicketNumber + ".pdf",
                    ObjectId = Case.InternalId,
                    Mime = "application/pdf",
                    Content = Convert.ToBase64String(content),
                    Incident = Case.ToModel(),
                    IncidentId = Case.SQLiteRecordId
                };
                if (connected)
                    try
                    {
                        #region Online Legalization Upload
                        if (!await CRMConnector.AddReportToIncident(note))
                        {
                            await _pageService.DisplayAlert("Error al subir legalización", "No se ha podido subir la legalización al servidor. Sin embargo, se creó una copia local en la carpeta recibos", "Ok");
                            IsBusy = false;
                            return;
                        }
                        else
                            NotificationService.DisplayMessage("Subida Correcta", "Se ha subido una legalización.");
                    }
                    catch (Exception ex)
                    {
                        if (await _pageService.DisplayAlert("Legalización enviada", "Se ha enviado la legalización con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                            await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                    }
                #endregion
                else
                #region Offline Legalization Save
                {
                    NotificationService.DisplayMessage("Guardado localmente", "Se sincronizará cuando se recupere la conexión.");
                    await CRMConnector.AddLegalizationReportOffline(note);
                }
                #endregion
            }
            #endregion
            #region Close Service Ticket
            vm.ProgressUp("Cerrando boleta de servicio");
            if (connected)
            {
                try
                {
                    if (!await CRMConnector.FinishServiceTicket(SelectedServiceTicket.ToModel()))
                    {
                        await _pageService.DisplayAlert("Error al cerrar Boleta de servicio", "No se ha podido cerrar la boleta de servicio", "Ok");
                        IsBusy = false;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (await _pageService.DisplayAlert("Boleta finalizada", "Se ha finalizado la boleta con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                        await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            else
                await CRMConnector.FinishServiceTicketOffline(SelectedServiceTicket.ToModel());
            vm.ProgressUp("Boleta finalizada");
            vm.IsLoading = false;
            NotificationService.DisplayMessage("Finalizada Correctamente", "Boleta local finalizada");
            #endregion
            _pageService.RemovePages(2);
            await _pageService.PopAsync();
            IsBusy = false;
        }

        /// <summary>
        /// Crea un reporte de servicio en modo de prevista de tal forma que el tecnico y el cliente puedan revisarlo antes de darlo por finalizado.
        /// </summary>
        /// <returns>Void</returns>
        private async Task CreatePreview()
        {
            try
            {
                #region Request Permissions
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                    if (results.ContainsKey(Permission.Storage))
                        status = results[Permission.Storage];
                }
                #endregion
                if (status == PermissionStatus.Granted)
                {
                    List<Note> photosmodel = new List<Note>();
                    foreach (NoteViewModel photo in _Photos)
                        photosmodel.Add(photo.ToModel());
                    switch (Case.Client.ReportType)
                    {
                        case Types.SPCCLIENT_REPORTTYPEOPTION.ContableSimple:
                            await PDFServices.CreateFinancialServiceTicketReport(SelectedServiceTicket.ToModel(), Case.ToModel(), photosmodel);
                            break;
                        case Types.SPCCLIENT_REPORTTYPEOPTION.Normal:
                        default:
                            await PDFServices.CreateServiceTicketReport(SelectedServiceTicket.ToModel(), Case.ToModel(), photosmodel);
                            break;
                    }
                }
                else
                    await _pageService.DisplayAlert("Sin permiso de almacenamiento.", "Por favor conceda el permiso para poder realizar esta acción.", "Ok");
            }catch(Exception ex)
            {
                if (await _pageService.DisplayAlert("Ha ocurrido un error", "Un error impide continuar la ejecución. Desea ver el detalle del error?", "Si", "No"))
                    await _pageService.DisplayAlert(ex.Message, ex.StackTrace, "Cerrar mensaje");
            }
        }
        #endregion
    }
}
