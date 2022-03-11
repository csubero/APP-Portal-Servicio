using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreateNewLegalizationPage : ContentPage
	{
		public CreateNewLegalizationPage () =>
			InitializeComponent ();

        private void CDTSearchText_Changed(object sender, TextChangedEventArgs e) =>
            (BindingContext as CreateNewLegalizationViewModel).ChangeCDTTextCommand?.Execute(null);

        private void IncidentSearchText_Changed(object sender, TextChangedEventArgs e) =>
           (BindingContext as CreateNewLegalizationViewModel).ChangeIncidentTextCommand?.Execute(null);
    }
}