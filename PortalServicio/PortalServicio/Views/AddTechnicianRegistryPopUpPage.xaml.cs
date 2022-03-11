using PortalServicio.Services;
using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddTechnicianRegistryPopUpPage : PopupPage
	{
		public AddTechnicianRegistryPopUpPage (ref CDTViewModel cdt, int scdtt)
		{
            BindingContext = new AddTechnicianRegistryViewModel(new PageService(),ref cdt, scdtt);
			InitializeComponent ();
		}

        private void Validate_SelectedIndexChanged(object sender, EventArgs e)
        {
            (BindingContext as AddTechnicianRegistryViewModel).ValidateInformationCommand?.Execute(null);
        }
    }
}