using PortalServicio.Services;
using PortalServicio.ViewModels;
using Xamarin.Forms;

namespace PortalServicio
{
    public partial class LoginPage : ContentPage
    {
        #region Constructors
        public LoginPage()
        {
            BindingContext = new LoginViewModel(new PageService());
            InitializeComponent();
            Loading();
        }
        #endregion

        protected override void OnAppearing()
        {
            (BindingContext as LoginViewModel).LoginCommand?.Execute(null);
            base.OnAppearing();
        }

        private async void Loading()
        {
            await Pgbar.RotateTo(360, 2000);
            Pgbar.Rotation = 0;
            if (Navigation.NavigationStack.Count == 1 && Pgbar.IsEnabled)
                Loading();
        }
    }
}
