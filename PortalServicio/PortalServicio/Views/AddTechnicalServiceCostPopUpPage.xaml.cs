using PortalServicio.Services;
using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddTechnicalServiceCostPopUpPage : PopupPage
	{
		public AddTechnicalServiceCostPopUpPage (ref IncidentViewModel incident, int selectedST)
		{
            BindingContext = new AddWorkCostsViewModel(new PageService(),ref incident, selectedST);
			InitializeComponent ();
		}
	}
}