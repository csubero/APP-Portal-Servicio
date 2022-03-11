using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CDTTicketSummaryPage : TabbedPage
	{
		public CDTTicketSummaryPage (ref CDTViewModel cdt, int indexOfTicket)
		{
            BindingContext = new CDTTicketSummaryViewModel(new PageService(), cdt, indexOfTicket);
			InitializeComponent ();
		}

        private void OpenPhoto(object sender, SelectedItemChangedEventArgs e) =>
         (BindingContext as CDTTicketSummaryViewModel).OpenPhotoCommand?.Execute(null);

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as CDTTicketSummaryViewModel).BackPressedCommand?.Execute(null);
            return base.OnBackButtonPressed();
        }
    }
}