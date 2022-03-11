using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTProjectMaterials : ContentPage
	{
		public CDTProjectMaterials () =>
			InitializeComponent ();

        private void TapProjecMaterial(object sender, SelectedItemChangedEventArgs e) =>
            (BindingContext as CDTSummaryViewModel).TapProjectElementCommand.Execute(null);
    }
}