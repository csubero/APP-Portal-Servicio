using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyLegalizationSummary : CarouselPage
    {
        public MyLegalizationSummary(LegalizationViewModel legalization)
        {
            BindingContext = new MyLegalizationSummaryViewModel(new PageService(), legalization);
            InitializeComponent();
        }
    }
}