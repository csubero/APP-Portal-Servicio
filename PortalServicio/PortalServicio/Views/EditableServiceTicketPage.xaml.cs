using PortalServicio.Services;
using PortalServicio.ViewModels;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditableServiceTicketPage : CarouselPage
    {
        #region Constructors
        public EditableServiceTicketPage(ref IncidentViewModel incident, int selectedServiceTicket, ObservableCollection<TechnicianViewModel> techs)
        {
            BindingContext = new EditableServiceTicketViewModel(new PageService(), ref incident, selectedServiceTicket,techs);
            InitializeComponent();
        }
        #endregion

        private void OpenPhoto(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as EditableServiceTicketViewModel).OpenPhotoCommand?.Execute(null);
        }

        private void DeleteProduct(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as EditableServiceTicketViewModel).DeleteProductCommand?.Execute(null);
        }
    }
}