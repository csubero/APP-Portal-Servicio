using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PortalServicio.ViewModels;
using PortalServicio.Services;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListProgramCasesPage : ContentPage
    {
        #region Constructor
        public ListProgramCasesPage()
        {
            BindingContext = new ListProgramCasesViewModel(new PageService());
            InitializeComponent();
        }
        #endregion

        #region Events
        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            (BindingContext as ListProgramCasesViewModel).FilterControlCommand?.Execute(null);
        }

        private void Filter_Toggle(object sender, ToggledEventArgs e)
        {
            (BindingContext as ListProgramCasesViewModel).FilterControlCommand?.Execute(null);
        }

        private void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as ListProgramCasesViewModel).OpenIncidentCommand?.Execute(null);
        }
    }
    #endregion
}