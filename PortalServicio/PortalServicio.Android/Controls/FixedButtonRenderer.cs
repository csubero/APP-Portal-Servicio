using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using PortalServicio.Droid.Controls;
using Android.Content;

[assembly: ExportRenderer(typeof(Button), typeof(FixedButtonRenderer))]
namespace PortalServicio.Droid.Controls
{
    public class FixedButtonRenderer : ButtonRenderer
    {
        public FixedButtonRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {

            }
        }
    }
}
