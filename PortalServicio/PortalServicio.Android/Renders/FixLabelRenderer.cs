using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using PortalServicio.Droid.Renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Label), typeof(FixLabelRenderer))]
namespace PortalServicio.Droid.Renders
{
    public class FixLabelRenderer : LabelRenderer
    {
        public FixLabelRenderer(Context context) : base(context)
        {
        }
    }
}