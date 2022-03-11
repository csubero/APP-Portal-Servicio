using PortalServicio.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PortalServicio.ViewModels;
using System.Collections.Generic;

namespace PortalServicio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignReportPage : ContentPage
    {       
        public SignReportPage(ref IncidentViewModel incident, int selectedST, List<NoteViewModel> photos)
        {
            InitializeComponent();
            BindingContext = new SignReportViewModel(new PageService(), ref incident, selectedST, ref signature, photos);            
        }
    }
}