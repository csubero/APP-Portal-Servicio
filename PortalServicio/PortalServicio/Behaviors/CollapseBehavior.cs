using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace PortalServicio.Behaviors
{
    public class CollapseBehavior : Behavior<ListView>
    {
        public static readonly BindableProperty NormalHeightProperty =
            BindableProperty.Create(nameof(NormalHeight), typeof(double), typeof(CollapseBehavior), 0.0d);
        public double NormalHeight { get { return (double)GetValue(NormalHeightProperty); } set { SetValue(NormalHeightProperty, value); } }

        public static readonly BindableProperty IsCollapsedProperty =
            BindableProperty.Create(nameof(IsCollapsed), typeof(bool), typeof(CollapseBehavior), true);
        public bool IsCollapsed { get { return (bool)GetValue(IsCollapsedProperty); } set { SetValue(IsCollapsedProperty, value); } }

        protected override void OnAttachedTo(ListView bindable)
        {
            base.OnAttachedTo(bindable);
            //bindable.BindingContextChanged += Bindable_BindingContextChanged;
            bindable.BindingContextChanged += (sender, _) =>
            {
                BindingContext = ((BindableObject)sender).BindingContext;
                if (IsCollapsed)
                {
                    var animation = new Animation(v => bindable.HeightRequest = v, bindable.Height, NormalHeight);
                    animation.Commit(bindable, "Collapse", 16, 250, Easing.SinInOut);
                }
                else
                {
                    var animation = new Animation(v => bindable.HeightRequest = v, bindable.Height, 0);
                    animation.Commit(bindable, "Expand", 16, 250, Easing.SinInOut);
                }
            };
        }

        private void Bindable_BindingContextChanged(object sender, EventArgs e)
        {
            ListView control = sender as ListView;
            BindingContext = ((BindableObject)sender).BindingContext;
            if (IsCollapsed)
            {
                var animation = new Animation(v => control.HeightRequest = v, control.Height, NormalHeight);
                animation.Commit(control, "Collapse", 16, 250, Easing.SinInOut);
            }
            else
            {
                var animation = new Animation(v => control.HeightRequest = v, control.Height, 0);
                animation.Commit(control, "Expand", 16, 250, Easing.SinInOut);
            }
        }

        //public static void IsCollapsedPropertyChange(BindableObject bindable, object oldValue, object newValue)
        //{
        //    if (bindable is Behavior<ListView> control)
        //        if ((bool)newValue)
        //        {

        //            //var animation = new Animation(v => control.HeightRequest = v, control.Height, 0);
        //            //animation.Commit(control, "Collapse", 16, 250, Easing.SinInOut);
        //        }
        //        else
        //        {
        //            //var animation = new Animation(v => control.HeightRequest = v, control.Height, 0);
        //            //animation.Commit(control, "Expand", 16, 250, Easing.SinInOut);
        //        }
        //}

        protected override void OnDetachingFrom(ListView bindable)
        {
            base.OnDetachingFrom(bindable);
        }
    }
}
