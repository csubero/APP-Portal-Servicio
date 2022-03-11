using System.ComponentModel;
using Android.Content;
using Android.Views;
using PortalServicio;
using PortalServicio.Droid.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedButton), typeof(ExtendedButtonRenderer))]
namespace PortalServicio.Droid.Controls
{
    public class ExtendedButtonRenderer : ButtonRenderer
    {
        public ExtendedButtonRenderer(Context context) : base(context)
        {
        }

        public new ExtendedButton Element
        {
            get
            {
                return (ExtendedButton)base.Element;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null)
                return;
            SetTextAlignment();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == ExtendedButton.HorizontalTextAlignmentProperty.PropertyName | e.PropertyName == ExtendedButton.VerticalTextAlignmentProperty.PropertyName)
                SetTextAlignment();
        }

        public void SetTextAlignment()
        {
            GravityFlags vertical = Element.VerticalTextAlignment.ToVerticalGravityFlags();
            GravityFlags horizontal = Element.HorizontalTextAlignment.ToHorizontalGravityFlags();
            Control.Gravity = horizontal | vertical;
            Control.SetTextSize(Android.Util.ComplexUnitType.Sp, Element.TextSize);
        }
    }

    public static class AlignmentHelper
    {
        public static GravityFlags ToHorizontalGravityFlags(this Xamarin.Forms.TextAlignment alignment)
        {
            if (alignment == Xamarin.Forms.TextAlignment.Center)
                return GravityFlags.CenterHorizontal;
            return alignment == Xamarin.Forms.TextAlignment.End ? GravityFlags.End : GravityFlags.Start;
        }

        public static GravityFlags ToVerticalGravityFlags(this Xamarin.Forms.TextAlignment alignment)
        {
            if (alignment == Xamarin.Forms.TextAlignment.Center)
                return GravityFlags.CenterVertical;
            return alignment == Xamarin.Forms.TextAlignment.End ? GravityFlags.Bottom : GravityFlags.Top;
        }
    }
}