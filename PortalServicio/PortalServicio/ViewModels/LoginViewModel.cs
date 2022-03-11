using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Plugin.Connectivity;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Properties
        private bool _IsBusy;
        private string _StatusMessage;
        private readonly IPageService _pageService;

        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { SetValue(ref _IsBusy, value); }
        }
        public string StatusMessage
        {
            get { return _StatusMessage; }
            set { SetValue(ref _StatusMessage, value); }
        }

        public ICommand LoginCommand { get; private set; }
        #endregion

        #region Constructors
        public LoginViewModel(IPageService pageService)
        {
            _pageService = pageService;
            LoginCommand = new Command(async () => await Login());
        }
        #endregion

        #region Events
        private async Task Login()
        {
            IsBusy = true;
            StatusMessage = "Autenticando";
            string username = String.Empty;
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    username = await CRMConnector.GetLoginUser();
                }
                catch (HttpRequestException)
                {

                    StatusMessage = "Iniciando localmente";
                    username = "Usuario local";
                }
                catch (AdalServiceException)
                {
                    await _pageService.DisplayAlert("Inicio sesión cancelado", "Has cancelado el inicio de sesión por lo que no es posible acceder al servicio", "Ok");
                }
            }
            else
                try
                {
                    username = (await CRMConnector.GetLoginUserOffline()).Name;
                }catch(Exception ex)
                {
                    await _pageService.DisplayAlert("Error", ex.Message,"Ok");
                    username = string.Empty;
                }
            if (!String.IsNullOrEmpty(username) && CRMConnector.Proxy.AccessToken != string.Empty && CRMConnector.Proxy.ExpiresOn > DateTime.Now)
            {
                StatusMessage = "Conectando al servidor";
                await _pageService.PushAsync(new OperationsPage(username));
            }
            else
                StatusMessage = "Se canceló inicio de sesión";
            IsBusy = false;
        }
        #endregion
    }
}
