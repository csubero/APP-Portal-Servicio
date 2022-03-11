using PortalServicio.Services;
using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using System.Collections.Generic;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackPopUpPage : PopupPage
    {
        public FeedbackPopUpPage(ref IncidentViewModel incident, int selectedServiceTicket, List<NoteViewModel> photos)
        {
            BindingContext = new FeedbackPopUpViewModel(new PageService(), ref incident, selectedServiceTicket, photos);
            InitializeComponent();
        }
    }
}