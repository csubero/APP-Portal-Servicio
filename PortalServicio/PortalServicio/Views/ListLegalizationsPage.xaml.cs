using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListLegalizationsPage : ContentPage
	{
		public ListLegalizationsPage ()
		{
			InitializeComponent ();
		}

        private void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        {
           
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
           
        }
    }
}