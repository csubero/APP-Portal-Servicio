using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class FeedbackPopUpViewModel : BaseViewModel
    {
        #region Properties
        private IncidentViewModel _Case;
        private ServiceTicketViewModel _SelectedServiceTicket;
        private List<NoteViewModel> _Photos;
        private bool _IsBusy;
        private readonly IPageService _pageService;
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
            }
        }
        public ref ServiceTicketViewModel RefSelectedServiceTicket
        {
            get { return ref _SelectedServiceTicket; }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }

        public ICommand SendFeedbackCommand { get; private set; }
        #endregion

        #region Constructors
        public FeedbackPopUpViewModel(IPageService pageService, ref IncidentViewModel incident, int selectedST, List<NoteViewModel> photos)
        {
            _pageService = pageService;
            _Photos = photos;
            Case = incident;
            SelectedServiceTicket = Case.ServiceTickets[selectedST];
            Case.FeedbackAnswer1 = 6;
            Case.FeedbackAnswer2 = 6;
            SendFeedbackCommand = new Command(async () => await SendFeedback());
        }
        #endregion

        #region Events
        /// <summary>
        /// Envía retroalimentación de cliente al servidor o la guarda localmente para ser enviada cuando se recupere la conexión a internet.
        /// </summary>
        /// <returns>Void</returns>
        private async Task SendFeedback()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    if (!await CRMConnector.SendReview(Case.ToModel(), SelectedServiceTicket.ToModel()))
                    {
                        IsBusy = false;
                        await _pageService.DisplayAlert("Error", "La comunicación con el servidor se interrumpió. Inténtelo de nuevo.", "Ok");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (await _pageService.DisplayAlert("Enviado", "Se ha guardado con uso de internet pero han habido problemas a nivel local", "Ver error", "No ver error"))
                        await _pageService.DisplayAlert("Error para programador", string.Format("{0}, ST: {1}", ex.Message, ex.StackTrace), "Ok");
                }
            }
            else
                await CRMConnector.SendReviewOffline(Case.ToModel(), SelectedServiceTicket.ToModel());
            SelectedServiceTicket.FeedbackSubmitted = true;
            await _pageService.PushAsync(new SignReportPage(ref RefCase, Case.ServiceTickets.IndexOf(SelectedServiceTicket), _Photos));
            await _pageService.PopUpPopAsync();
            IsBusy = false;
        }
        #endregion
    }
}
