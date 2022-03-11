using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PortalServicio
{
    [ContentProperty("ColorRGBA")]
    class CustomColor : IMarkupExtension
    {
        public string ColorRGBA { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (String.IsNullOrWhiteSpace(ColorRGBA))
                return null;
            string[] decompose = ColorRGBA.Split(':');
            return new Color(Double.Parse(decompose[0])/255, Double.Parse(decompose[1])/255, Double.Parse(decompose[2])/255, Double.Parse(decompose[3])/255);
        }
    }
}
