using Xamarin.Forms;

namespace PortalServicio.Extensions
{
    public static class ProgressBarAnimationAttached
    {
        public static readonly BindableProperty AnimatedProgressProperty =
           BindableProperty.CreateAttached("AnimatedProgress",
                                           typeof(decimal),
                                           typeof(ProgressBar),
                                           (decimal)0.0d,
                                           BindingMode.OneWay,
                                           propertyChanged: (b, o, n) =>
                                           ProgressBarProgressChanged((ProgressBar)b, (decimal)n));

        private async static void ProgressBarProgressChanged(ProgressBar progressBar, decimal progress)
        {
            ViewExtensions.CancelAnimations(progressBar);
            await progressBar.ProgressTo((double)progress, 800, Easing.SinOut);
        }

        public static decimal GetAnimatedProgress(BindableObject view)
        {
            return (decimal)view.GetValue(AnimatedProgressProperty);
        }

        public static void SetAnimatedProgress(BindableObject view, decimal value)
        {
            view.SetValue(AnimatedProgressProperty, value);
        }
    }
}
