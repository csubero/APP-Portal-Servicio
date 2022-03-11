using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTSummaryPage : TabbedPage
	{
		public CDTSummaryPage (CDTViewModel cdt)
		{
            BindingContext = new CDTSummaryViewModel(new PageService(),cdt);
			InitializeComponent ();
		}
    }
}