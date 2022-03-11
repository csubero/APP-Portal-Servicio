using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using PortalServicio.Controls;
using PortalServicio.iOS.Controls;
using UIKit;
using CoreGraphics;

[assembly: ExportRenderer(typeof(GlassEditor), typeof(GlassEditorRenderer))]
namespace PortalServicio.iOS.Controls
{
    class GlassEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                GlassEditor xfEditor = e.NewElement as GlassEditor;
                if (string.IsNullOrEmpty(Control.Text))
                {
                    Control.Text = xfEditor.Placeholder;
                    Control.Layer.BorderColor = new CGColor(UIColor.LightGray.CGColor,0.5f);// UIColor.LightGray.CGColor;
                }
                else
                {
                    Control.Layer.BorderColor = UIColor.Black.CGColor;
                }

                Control.Layer.BorderWidth = 1f;
                Control.Layer.CornerRadius = 7f;
                Control.ShouldBeginEditing = t =>
                {
                    if (Control.Text == xfEditor.Placeholder)
                    {
                        Control.Text = string.Empty;
                        Control.TextColor = UIColor.Black;
                        Control.Layer.BorderColor = UIColor.Black.CGColor;
                    }
                    return true;
                };
                Control.ShouldEndEditing = t =>
                {
                    if (string.IsNullOrEmpty(Control.Text))
                    {
                        Control.Text = xfEditor.Placeholder;
                        Control.TextColor = UIColor.LightGray;
                        Control.Layer.BorderColor = UIColor.LightGray.CGColor;
                    }
                    return true;
                };
                Control.ScrollEnabled = false;
            }
        }
    }
}