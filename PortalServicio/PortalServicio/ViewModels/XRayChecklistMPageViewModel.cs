using Plugin.Connectivity;
using PortalAPI.Contracts;
using PortalServicio.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class XRayChecklistMPageViewModel : BaseViewModel
    {
        #region Properties
        private ServiceTicketViewModel _Ticket;
        private bool _IsBusy;
        private Dictionary<string, Types.SPCSERVTICKET_HWSTATE> _Dic_HWState;
        private Dictionary<string, Types.SPCSERVTICKET_LEADSTATE> _Dic_LeadState;
        private Dictionary<string, Types.SPCSERVTICKET_SCREENTYPE> _Dic_ScreenType;
        private Dictionary<string, Types.SPCSERVTICKET_RADSTATE> _Dic_RadState;
        private Dictionary<string, Types.SPCSERVTICKET_VISITNUMBER> _Dic_VisitNumber;
        private Dictionary<string, Types.SPCSERVTICKET_POSSESIONSTATE> _Dic_PossesionState;
        private Dictionary<string, Types.SPCSERVTICKET_TECHNOLOGY> _Dic_Technology;
        private readonly IPageService _pageService;

        public ServiceTicketViewModel Ticket
        {
            get { return _Ticket; }
            set { SetValue(ref _Ticket, value); }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_HWSTATE> Dic_HWState
        {
            get { return _Dic_HWState; }
            private set { SetValue(ref _Dic_HWState, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_HWSTATE>> Dic_HWStateList
        {
            get { return _Dic_HWState.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_LEADSTATE> Dic_LeadState
        {
            get { return _Dic_LeadState; }
            private set { SetValue(ref _Dic_LeadState, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_LEADSTATE>> Dic_LeadStateList
        {
            get { return _Dic_LeadState.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_SCREENTYPE> Dic_ScreenType
        {
            get { return _Dic_ScreenType; }
            private set { SetValue(ref _Dic_ScreenType, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_SCREENTYPE>> Dic_ScreenTypeList
        {
            get { return _Dic_ScreenType.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_RADSTATE> Dic_RadState
        {
            get { return _Dic_RadState; }
            private set { SetValue(ref _Dic_RadState, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_RADSTATE>> Dic_RadStateList
        {
            get { return _Dic_RadState.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_VISITNUMBER> Dic_VisitNumber
        {
            get { return _Dic_VisitNumber; }
            private set { SetValue(ref _Dic_VisitNumber, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_VISITNUMBER>> Dic_VisitNumberList
        {
            get { return _Dic_VisitNumber.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_POSSESIONSTATE> Dic_PossesionState
        {
            get { return _Dic_PossesionState; }
            private set { SetValue(ref _Dic_PossesionState, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_POSSESIONSTATE>> Dic_PossesionStateList
        {
            get { return _Dic_PossesionState.ToList(); }
        }
        public Dictionary<string, Types.SPCSERVTICKET_TECHNOLOGY> Dic_Technology
        {
            get { return _Dic_Technology; }
            private set { SetValue(ref _Dic_Technology, value); }
        }
        public List<KeyValuePair<string, Types.SPCSERVTICKET_TECHNOLOGY>> Dic_TechnologyList
        {
            get { return _Dic_Technology.ToList(); }
        }

        public ICommand SaveChangesCommand { get; private set; }
        #endregion

        #region Constructors
        public XRayChecklistMPageViewModel(IPageService pageService, ref ServiceTicketViewModel serviceTicket)
        {
            Ticket = serviceTicket;
            _pageService = pageService;
            SaveChangesCommand = new Command(async () => await SaveChanges());
            Dic_HWState = new Dictionary<string, Types.SPCSERVTICKET_HWSTATE>
            {
                { "Normal", Types.SPCSERVTICKET_HWSTATE.Normal },
                { "Falla", Types.SPCSERVTICKET_HWSTATE.Falla }
            };
            Dic_LeadState = new Dictionary<string, Types.SPCSERVTICKET_LEADSTATE>
            {
                { "Ok", Types.SPCSERVTICKET_LEADSTATE.Ok },
                { "Regular", Types.SPCSERVTICKET_LEADSTATE.Reg },
                { "Mal", Types.SPCSERVTICKET_LEADSTATE.Mal }
            };
            Dic_ScreenType = new Dictionary<string, Types.SPCSERVTICKET_SCREENTYPE>
            {
                { "CRT", Types.SPCSERVTICKET_SCREENTYPE.CRT },
                { "LCD", Types.SPCSERVTICKET_SCREENTYPE.LCD }
            };
            Dic_RadState = new Dictionary<string, Types.SPCSERVTICKET_RADSTATE>
            {
                { "Cumple", Types.SPCSERVTICKET_RADSTATE.Cumple },
                { "No Cumple", Types.SPCSERVTICKET_RADSTATE.NoCumple }
            };
            Dic_VisitNumber = new Dictionary<string, Types.SPCSERVTICKET_VISITNUMBER>
            {
                { "Visita 1", Types.SPCSERVTICKET_VISITNUMBER.Visita1 },
                { "Visita 2", Types.SPCSERVTICKET_VISITNUMBER.Visita2 },
                { "Visita 3", Types.SPCSERVTICKET_VISITNUMBER.Visita3 },
                { "Visita 4", Types.SPCSERVTICKET_VISITNUMBER.Visita4 }
            };
            Dic_PossesionState = new Dictionary<string, Types.SPCSERVTICKET_POSSESIONSTATE>
            {
                { "Activo", Types.SPCSERVTICKET_POSSESIONSTATE.Enabled },
                { "Inactivo", Types.SPCSERVTICKET_POSSESIONSTATE.Disabled },
                { "No tiene", Types.SPCSERVTICKET_POSSESIONSTATE.NotHave }
            };
            Dic_Technology = new Dictionary<string, Types.SPCSERVTICKET_TECHNOLOGY>
            {
                { "HiTrax", Types.SPCSERVTICKET_TECHNOLOGY.HiTrax },
                { "SiProx", Types.SPCSERVTICKET_TECHNOLOGY.SiProx }
            };
        }
        #endregion

        #region Events
        private async Task SaveChanges()
        {
            if (IsBusy) return;
            IsBusy = true;
            NotificationService.DisplayMessage("Guardando", "Se envía los cambios al servidor.");
            if (CrossConnectivity.Current.IsConnected)
                if (await CRMConnector.SaveChangesOfXRC(Ticket.ToModel()))
                    NotificationService.DisplayMessage("Guardado", "Los cambios se guardaron satisfactoriamente");
                else
                    await _pageService.DisplayAlert("No se pudo guardar", "No se pudo guardar los cambios. Intente de nuevo y si el problema persiste contacte al administrador.", "Ok");
            else
            {
                await CRMConnector.SaveChangesOfXRCOffline(Ticket.ToModel());
                NotificationService.DisplayMessage("Guardado localmente", "Los cambios se guardaron satisfactoriamente");
            }
            IsBusy = false;
        }
        #endregion
    }
}
