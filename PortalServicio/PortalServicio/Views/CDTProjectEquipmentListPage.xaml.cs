using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTProjectEquipmentListPage : ContentPage
	{
		public CDTProjectEquipmentListPage () =>
			InitializeComponent ();

        private void TapProjectEquipment(object sender, SelectedItemChangedEventArgs e) =>
            (BindingContext as CDTSummaryViewModel).TapProjectElementCommand.Execute(null);
    }
}