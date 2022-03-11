using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using PortalServicio.Controls;
using PortalServicio.iOS.Controls;
using UIKit;

[assembly: ExportRenderer(typeof(GlassEntry), typeof(GlassEntryRenderer))]
namespace PortalServicio.iOS.Controls
{
    class GlassEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // do whatever you want to the UITextField here!
                Control.BackgroundColor = UIColor.FromRGBA(255, 255, 255,100);
                Control.BorderStyle = UITextBorderStyle.RoundedRect;
            }
        }     
    }
}