using System;
using CoreLocation;
using PortalServicio.iOS;
using PortalServicio.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CommonDeviceFuncs))]
namespace PortalServicio.iOS
{
    class CommonDeviceFuncs : ICommonDeviceFuncs
    {
        public double CalculateDistance(double fromlat, double fromlon, double tolat, double tolon)
        {
            CLLocation pointA = new CLLocation(fromlat, fromlon);
            CLLocation pointB = new CLLocation(tolat, tolon);
            return pointB.DistanceFrom(pointA);
        }
    }
}