using System;
using Xamarin.Forms;

namespace PortalServicio.AttachedProperties
{
    public class AttachedCollapse
    {
        private static double _RealHeight;
        public static readonly BindableProperty IsCollapsedProperty =
            BindableProperty.CreateAttached(
                "IsCollapsed",
                typeof(bool),
                typeof(AttachedCollapse),
                false,
                propertyChanged: IsCollapsedChanged);

        private static void IsCollapsedChanged(BindableObject view, object oldValue, object newValue)
        {
            if (view is ListView control)
                if ((bool)newValue)
                {
                    _RealHeight = control.Height;
                    var animation = new Animation(v => control.HeightRequest = v, control.Height, 0);
                    animation.Commit(control, "Collapse", 16, 250, Easing.SinInOut);
                }
                else
                {
                    var animation = new Animation(v => control.HeightRequest = v, control.Height, _RealHeight);
                    animation.Commit(control, "Expand", 16, 250, Easing.SinInOut);
                }
        }

        public static bool GetIsCollapsed(BindableObject view)
        {
            return (bool)view.GetValue(IsCollapsedProperty);
        }

        public static void SetIsCollapsed(BindableObject view, bool value)
        {
            view.SetValue(IsCollapsedProperty, value);
        }
    }
}
