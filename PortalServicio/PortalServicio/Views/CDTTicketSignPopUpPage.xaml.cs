using PortalServicio.Services;
using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using System;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTTicketSignPopUpPage : PopupPage
	{
		public CDTTicketSignPopUpPage (ref CDTViewModel cdt, int index, TimeSpan ellapsed)
		{
           
            InitializeComponent();
            BindingContext = new CDTTicketSignPopUpViewModel(new PageService(), index, ref cdt, ellapsed, ref signature);
            //(BindingContext as CDTTicketSignPopUpViewModel).SetSignatureComponent(ref signature);
        }
	}
}