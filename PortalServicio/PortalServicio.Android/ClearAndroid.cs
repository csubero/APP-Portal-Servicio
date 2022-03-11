using Xamarin.Forms;
using PortalServicio.Services;
using PortalServicio.Droid;
using Android.Webkit;

[assembly: Dependency(typeof(ClearAndroid))]
namespace PortalServicio.Droid
{
    class ClearAndroid : IClear
    {
        public void ClearCache()
        {
            CookieManager.Instance.RemoveAllCookie();
        }
    }
}