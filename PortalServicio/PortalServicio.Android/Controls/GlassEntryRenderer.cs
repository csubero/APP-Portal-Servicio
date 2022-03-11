using Android.Support.V4.Content;
using PortalServicio.Controls;
using PortalServicio.Droid.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(GlassEntry), typeof(GlassEntryRenderer))]
namespace PortalServicio.Droid.Controls
{
    class GlassEntryRenderer : EntryRenderer
    {
#pragma warning disable CS0618 // Type or member is obsolete
        public GlassEntryRenderer() : base()
#pragma warning restore CS0618 // Type or member is obsolete
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = ContextCompat.GetDrawable(this.Context, Resource.Drawable.RoundedEntry); //Resources.GetDrawable(Resource.Drawable.RoundedEntry);
                //Control.SetBackgroundColor(global::Android.Graphics.Color.LightGreen);
                Control.Background.SetAlpha(100);               
            }
        }
    }
}