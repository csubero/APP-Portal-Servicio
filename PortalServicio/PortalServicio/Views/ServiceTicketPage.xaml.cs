using PortalServicio.Models;
using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServiceTicketPage : CarouselPage
    {
        #region Constructors
        public ServiceTicketPage(ref IncidentViewModel incident, int selectedServiceTicket)
        {
            BindingContext = new ReadOnlyServiceTicketViewModel(new PageService(), ref incident, selectedServiceTicket);
            InitializeComponent();
        }
        #endregion

        private void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            ((ListView)sender).SelectedItem = null;
        }

        private async void OpenPhoto(object sender, SelectedItemChangedEventArgs e)
        {
            await (BindingContext as ReadOnlyServiceTicketViewModel).OpenPhoto();
        }
    }
}