using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTTicketTechniciansRegistryListPage : ContentPage
	{
		public CDTTicketTechniciansRegistryListPage () =>
			InitializeComponent ();

        private void SelectRegistry(object sender, SelectedItemChangedEventArgs e) =>
         (BindingContext as CDTTicketSummaryViewModel).ToggleCollapseTechnicianRegistryCommand?.Execute(null);
    }
}