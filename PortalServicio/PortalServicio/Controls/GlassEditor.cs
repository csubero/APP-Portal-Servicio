using System;
using Xamarin.Forms;

namespace PortalServicio.Controls
{
    public class GlassEditor : Editor
    {
        public static readonly BindableProperty PlaceholderProperty = 
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(GlassEditor), String.Empty);

        public GlassEditor()
        {
            this.TextChanged += (sender, e) =>
            {
                this.InvalidateMeasure();
            };
        }

        public string Placeholder
        {
            get
            {
                return (string)GetValue(PlaceholderProperty);
            }

            set
            {
                SetValue(PlaceholderProperty, value);
            }
        }
    }
}