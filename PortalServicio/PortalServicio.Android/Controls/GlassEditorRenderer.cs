using Xamarin.Forms;
using PortalServicio.Controls;
using PortalServicio.Droid.Controls;
using Xamarin.Forms.Platform.Android;
using Android.Support.V4.Content;
using System.ComponentModel;
using Android.Graphics.Drawables;

[assembly: ExportRenderer(typeof(GlassEditor), typeof(GlassEditorRenderer))]
namespace PortalServicio.Droid.Controls
{
    class GlassEditorRenderer : EditorRenderer
    {
#pragma warning disable CS0618 // Type or member is obsolete
        public GlassEditorRenderer() : base()
#pragma warning restore CS0618 // Type or member is obsolete
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
            Control.Background = ContextCompat.GetDrawable(this.Context, Resource.Drawable.RoundedEntry);
            Control.Background.SetAlpha(100);         
            if (e.NewElement != null)
            {
                var element = e.NewElement as GlassEditor;
                Control.Hint = element.Placeholder;
            }
        }
    }

    protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnElementPropertyChanged(sender, e);
        if (e.PropertyName == GlassEditor.PlaceholderProperty.PropertyName)
        {
            var element = this.Element as GlassEditor;
            Control.Hint = element.Placeholder;
            }
    }

}
}