using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyLegalizationsPage : ContentPage
	{
		public MyLegalizationsPage () =>
			InitializeComponent ();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as MyLegalizationsViewModel).LoadLegalizationsCommand?.Execute(null);
        }
    }
}