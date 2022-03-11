using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CDTTicketListPage : ContentPage
    {
        public CDTTicketListPage() =>
            InitializeComponent();

        private void SelectTicket(object sender, SelectedItemChangedEventArgs e) =>
          (BindingContext as CDTSummaryViewModel).OpenTicketCommand?.Execute(null);
    }
}