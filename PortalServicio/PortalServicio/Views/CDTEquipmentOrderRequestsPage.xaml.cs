using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTEquipmentOrderRequestsPage : ContentPage
	{
		public CDTEquipmentOrderRequestsPage () =>
			InitializeComponent ();

        private void TapProjecMaterial(object sender, SelectedItemChangedEventArgs e) =>
            (BindingContext as CDTSummaryViewModel).TapProjectElementCommand.Execute(null);

        private void EquipmentRequestCollapse(object sender, SelectedItemChangedEventArgs e) =>
           (BindingContext as CDTSummaryViewModel).ToggleCollapseEquipmentCommand.Execute(null);
    }
}