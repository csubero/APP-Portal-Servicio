using Xamarin.Forms;
using Foundation;
using UIKit;
using Syncfusion.SfPdfViewer.XForms.iOS;
using Plugin.Toasts;
using UserNotifications;
using Syncfusion.SfRating.XForms.iOS;
using Syncfusion.SfGauge.XForms.iOS;
using CarouselView.FormsPlugin.iOS;

namespace PortalServicio.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        UIWindow window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

            var application = new App();
            window = new UIWindow(UIScreen.MainScreen.Bounds)
            {
                RootViewController = application.MainPage.CreateViewController()
            };
            CRMConnector.uiViewController = window.RootViewController;
            window.MakeKeyAndVisible();
            CarouselViewRenderer.Init();
            new SfPdfDocumentViewRenderer();
            new SfGaugeRenderer();
            new SfRatingRenderer();
            LoadApplication(application);
            DependencyService.Register<ToastNotification>();
            ToastNotification.Init();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Request Permissions
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (granted, error) =>
                {
                    // Do something if needed
                });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);

                app.RegisterUserNotificationSettings(notificationSettings);
            }
            //return base.FinishedLaunching (app, options);
            return true;
		}

        //public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        //{
        //    var application = new CrmXForm.App();
        //    window = new UIWindow(UIScreen.MainScreen.Bounds);
        //    window.RootViewController = application.MainPage.CreateViewController();
        //    window.MakeKeyAndVisible();

        //    CrmXFormHelper.uiViewController = window.RootViewController;

        //    LoadApplication(application);
        //    return true;
        //}
    }
}
