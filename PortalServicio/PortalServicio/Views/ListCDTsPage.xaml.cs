using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListCDTsPage : ContentPage
	{
		public ListCDTsPage ()
		{
            BindingContext = new ListCDTsViewModel(new PageService());
            InitializeComponent ();
		}
    }
}