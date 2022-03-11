using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xamarin.Forms;
using Plugin.Toasts;
using Plugin.Permissions;
using CarouselView.FormsPlugin.Android;

namespace PortalServicio.Droid
{
	[Activity (Label = "PortalServicio", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar; 

			base.OnCreate (bundle);
            CRMConnector.activity = this;
            Forms.Init (this, bundle);
            CarouselViewRenderer.Init();
            DependencyService.Register<ToastNotification>();       
            ToastNotification.Init(this, new PlatformOptions() { Style= NotificationStyle.Snackbar});
            LoadApplication (new App());
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            // Pass the authentication result to ADAL. 
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

