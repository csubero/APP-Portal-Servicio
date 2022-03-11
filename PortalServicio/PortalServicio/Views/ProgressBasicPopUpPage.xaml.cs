using PortalServicio.ViewModels;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProgressBasicPopUpPage : PopupPage
	{
		public ProgressBasicPopUpPage (ref ProgressBasicPopUpViewModel vm)
		{
            BindingContext = vm;
			InitializeComponent();
		}
	}
}