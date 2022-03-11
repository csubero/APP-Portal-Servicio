using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateTicketPage : ContentPage
    {
        public CreateTicketPage()
        {
            BindingContext = new CreateTicketViewModel(new PageService());
            InitializeComponent();
        }

        private void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as CreateTicketViewModel).SelectClientCommand?.Execute(null);
        }
    }
}