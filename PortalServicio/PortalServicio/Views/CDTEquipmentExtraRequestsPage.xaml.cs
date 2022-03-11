using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTEquipmentExtraRequestsPage : ContentPage
	{
		public CDTEquipmentExtraRequestsPage () =>
			InitializeComponent ();

        private void SelectExtraEquipment(object sender, SelectedItemChangedEventArgs e) =>
         (BindingContext as CDTSummaryViewModel).SelectExtraEquipmentCommand?.Execute(null);
    }
}