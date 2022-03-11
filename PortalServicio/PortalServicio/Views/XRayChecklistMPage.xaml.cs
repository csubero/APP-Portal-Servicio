using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class XRayChecklistMPage : TabbedPage
    {
        public XRayChecklistMPage (ref ServiceTicketViewModel serviceTicket)
        {
            InitializeComponent();
            BindingContext = new XRayChecklistMPageViewModel(new PageService(), ref serviceTicket);
        }
    }
}