using PortalServicio.Services;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PortalServicio.ViewModels;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgramCasePage : ContentPage
    {
        public ProgramCasePage(IncidentViewModel selectedIncident, IEnumerable<TechnicianViewModel> techs)
        {
            BindingContext = new ProgramCaseViewModel(new PageService(), ref selectedIncident, techs);
            InitializeComponent();
        }

        private void OpenNote(object sender, SelectedItemChangedEventArgs e)
        {
            (BindingContext as ProgramCaseViewModel).OpenNoteCommand?.Execute(null);
        }
    }
}