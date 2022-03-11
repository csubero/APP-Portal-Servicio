using Xamarin.Forms;
using PortalServicio.ViewModels;
using PortalServicio.Services;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCasesPage : ContentPage
    {
        #region Constructor
        public ListCasesPage()
        {
            BindingContext = new ListCasesViewModel(new PageService());
            InitializeComponent();
        }
        #endregion
    }
}