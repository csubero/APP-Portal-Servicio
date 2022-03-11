using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using PortalServicio.Services;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace PortalServicio
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
            CrossConnectivity.Current.ConnectivityTypeChanged += ConnectivityTypeChanged;
        }

        async void ConnectivityTypeChanged(object sender, ConnectivityTypeChangedEventArgs args)
        {
            if (args.ConnectionTypes.Any() && !args.IsConnected)
            {
                var connected = await CrossConnectivity.Current.AwaitConnected(TimeSpan.FromSeconds(10));
                if (connected)
                {
                    NotificationService.DisplayMessage("Conectado", "Se ha recuperado la conexión a internet");
                    await CRMConnector.SyncOperations();
                }
                else
                    NotificationService.DisplayMessage("Desconectado", "Se ha perdido la conexión a internet");
            }
            else
                NotificationService.DisplayMessage("Desconectado", "Se ha perdido la conexión a internet");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
