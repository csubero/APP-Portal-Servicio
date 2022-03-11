using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace PortalServicio.Services
{
    public static class GeoLocationService
    {
        public static async Task<Tuple<double,double>> GetCurrentLocation()
        {
            Position pos = new Position();
            if (CrossGeolocator.Current.IsGeolocationEnabled && CrossGeolocator.IsSupported)
                try
                {
                    pos = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0, 0, 10));
                }
                catch(TaskCanceledException)
                {
                    throw new GeolocationException(GeolocationError.PositionUnavailable);
                }
            else
                throw new GeolocationException(GeolocationError.PositionUnavailable);
            return Tuple.Create(pos.Latitude, pos.Longitude);
        }

        public static async Task<Tuple<double, double>> GetLastKnownLocation()
        {
            Position pos = new Position();
            if (CrossGeolocator.Current.IsGeolocationEnabled)
                pos = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            else
                throw new GeolocationException(GeolocationError.PositionUnavailable);
            return Tuple.Create(pos.Latitude, pos.Longitude);
        }

        public static double GetDistance(double lat, double lon, double centerlat,double centerlon, int radius)
        {
            if (lat == centerlat && lon == centerlon)
                return 0;
            return DependencyService.Get<ICommonDeviceFuncs>().CalculateDistance(lat, lon, centerlat, centerlon);
        }
    }
}
