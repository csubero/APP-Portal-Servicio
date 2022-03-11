using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk.Query.Samples;
using Microsoft.Xrm.Sdk.Samples;
using System;
using System.Threading.Tasks;
using PortalServicio.Configuration;
using PortalServicio.Models;

namespace PortalServicio
{
    // Inherit from OrganizationDataWebServiceProxy. 
    public class OrganizationProxy : OrganizationDataWebServiceProxy
    {
        public DateTimeOffset ExpiresOn { get; set; }
        public SystemUser LoggedUser { get; set; }
        #region Method     

        public OrganizationProxy()
        {
            ServiceUrl = ServerUri;
        }

        // Wrap SDK methods. This example uses Execute and Retrieve method only. 
        new public async Task<OrganizationResponse> Execute(OrganizationRequest request)
        {
            // Aquire Token before every call. 
            await Authenticate();
            // I omit try/catch error handling to make sample simple. 
            return await base.Execute(request);
        }

        new public async Task<Entity> Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            await Authenticate();
            return await base.Retrieve(entityName, id, columnSet);
        }

        public async Task Authenticate()
        {
            // Make sure AccessToken is valid. 
            await GetTokenSilent();

            // Wait until AccessToken assigned 
            while (String.IsNullOrEmpty(AccessToken))
            {
                await Task.Delay(10);
            }
        }

        #endregion

        #region ADAL

        #region Property

        public AuthenticationContext authContext = null;
#if __ANDROID__
        public Android.App.Activity activity;
#elif __IOS__
        public UIKit.UIViewController uiViewController; 
#endif
        private string OAuthUrl = Config.AUTHORITY;
        private string ServerUri = Config.RESOURCE;
        private string ClientId = Config.CLIENT_ID;
        private string RedirectUri = Config.REDIRECT_URI;
        #endregion

        #region Method

        private async Task GetTokenSilent()
        {
            // If no authContext, then create it. 
            if (authContext == null)
            {
                authContext = new AuthenticationContext(OAuthUrl,false);
            }

            AuthenticationResult result = null;

#if __ANDROID__ 
            IPlatformParameters parameters = new PlatformParameters(activity);
#elif __IOS__
            IPlatformParameters parameters = new PlatformParameters(uiViewController); 
#endif
            //try
            //{
                result = await authContext.AcquireTokenAsync(ServerUri, ClientId, new Uri(RedirectUri), parameters);
                StoreToken(result);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }

        /// <summary> 
        /// This mothod called when ADAL obtained AccessToken 
        /// </summary> 
        /// <param name="result"></param> 
        private void StoreToken(AuthenticationResult result)
        {
            AccessToken = result.AccessToken;
            ExpiresOn = result.ExpiresOn;
        }

        public void DeleteToken()
        {
            authContext.TokenCache.Clear();          
        }
        #endregion

        #endregion 
    }
}