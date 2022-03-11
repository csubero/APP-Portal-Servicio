using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
    public class CDTTicketSignPopUpViewModel : BaseViewModel
    {
        private readonly IPageService _pageService;
        private CDTTicketViewModel _SelectedTicket;
        private CDTViewModel _CDT;
        private bool _IsBusy;
        private TimeSpan _EllapsedTime;
        private SignaturePadView _SignaturePad;
        private string _Name;
        private string _Identification;

        public CDTTicketViewModel SelectedTicket
        {
            get { return _SelectedTicket; }
            set { SetValue(ref _SelectedTicket, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }
        public string Identification
        {
            get { return _Identification; }
            set { SetValue(ref _Identification, value); }
        }
        public CDTViewModel CDT
        {
            get { return _CDT; }
            set { SetValue(ref _CDT, value); }
        }
        public ref CDTViewModel RefCDT
        {
            get { return ref _CDT; }
        }

        public ICommand SignTicketCommand { get; private set; }

        public CDTTicketSignPopUpViewModel(IPageService pageService,int ticket, ref CDTViewModel cdt, TimeSpan ellapsed, ref SignaturePadView signpad)
        {
            _pageService = pageService;
            _EllapsedTime = ellapsed;
            CDT = cdt;
            _SignaturePad = signpad;
            SelectedTicket = CDT.CDTTickets[ticket];
            SignTicketCommand = new Command(async () => await CloseTicket());
        }

        //public void SetSignatureComponent(ref SignaturePadView signpad)
        //{
        //    _SignaturePad = signpad;
        //}

        private async Task CloseTicket()
        {
            if (IsBusy)
                return;
            if (SelectedTicket.TechniciansRegistered.Count == 0)
            {
                await _pageService.DisplayAlert("Necesitas asignar técnicos", "Debes agregar al menos 1 técnico para cerrar la boleta", "Comprendo");
                return;
            }
            IsBusy = true;
            #region Obtain Sign
            ImageConstructionSettings settings = new ImageConstructionSettings
            {
                BackgroundColor = Color.White,
                StrokeColor = Color.Black,
                DesiredSizeOrScale = new SizeOrScale(150, 50, SizeOrScaleType.Size)
            };
            Stream stream = await _SignaturePad.GetImageStreamAsync(SignatureImageFormat.Jpeg, settings);
            #endregion
            SelectedTicket.Finished = SelectedTicket.Started + _EllapsedTime;
            foreach (TechnicianRegistryViewModel tech in SelectedTicket.TechniciansRegistered)
            {
                if (!tech.IsDatetimeSet)
                    tech.Finished = SelectedTicket.Finished;
                List<DateTime> holydays = await CRMConnector.GetAllHolydays();
                double[] hours = Configuration.Config.CalculateHours(tech.Started, tech.Finished, holydays);
                tech.HoursNormal = hours[0];
                tech.HoursNormalNight = hours[2];
                tech.HoursDaytimeExtra = hours[1];
                tech.HoursNightExtra = hours[3];
                tech.HoursHolydayDaytime = hours[4];
                tech.HoursHolydayNight = hours[5];
                tech.HoursOffdayDaytime = hours[6];
                tech.HoursOffdayNight = hours[7];
                tech.HoursOffdayDaytimeExtra = hours[8];
                tech.HoursOffdayNightExtra = hours[9];
            }
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
                await PDFServices.CreateCDTTicketReport(SelectedTicket.ToModel(), CDT.ToModel(), stream, Name, Identification);
                byte[] doc = DependencyService.Get<ISave>().GetDocumentBytesAsync("BoletaDeCDT.pdf");
                Note STDoc = new Note
                {
                    Filename = "ReporteDeCDT" + SelectedTicket.Number + ".pdf",
                    ObjectId = CDT.InternalId,
                    Mime = "application/pdf",
                    Content = Convert.ToBase64String(doc),
                    CDT = CDT.ToModel(),
                    CDTId = CDT.SQLiteRecordId
                };
                if (!STDoc.ObjectId.Equals(default(Guid)))
                {
                    try
                    {
                        if (!await CRMConnector.AddReportToCDT(STDoc))
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
                        if (await _pageService.DisplayAlert("Envío fallido", "Se ha enviado el reporte con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                            await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                    }
                }
                else
                {
                    NotificationService.DisplayMessage("Error", "Sucedio un error al subir el reporte");
                    //NotificationService.DisplayMessage("Guardado localmente", "Se sincronizará cuando se recupere la conexión.");
                    //await CRMConnector.AddServiceTicketSignedOffline(STDoc);
                }
                if (await CRMConnector.FinishCDTTicket(SelectedTicket.ToModel()))
                {
                    await _pageService.PopUpPopAsync();
                    await _pageService.PopAsync();
                }
                else
                {
                    SelectedTicket.Finished = default(DateTime);
                    await _pageService.DisplayAlert("No se pudo finalizar", "Un error impide la finalización de esta visita. Desea ver el error?", "Ok");
                }
            }
            else
                NotificationService.DisplayMessage("Sin permisos", "Se requiere permitir acceso a memoria del dispositivo para crear el pdf.");
            IsBusy = false;
        }
    }
}
