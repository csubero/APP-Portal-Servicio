using System;
using Android.Locations;
using PortalServicio.Droid;
using PortalServicio.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CommonDeviceFuncs))]
namespace PortalServicio.Droid
{
    class CommonDeviceFuncs : ICommonDeviceFuncs
    {
        public double CalculateDistance(double fromlat, double fromlon, double tolat, double tolon)
        {
            float[] answers = new float[3];
            Location.DistanceBetween(fromlat, fromlon, tolat, tolon, answers);
            if (answers[0] == default(float))
                throw new Exception("Error al calcular distancia.");
            return answers[0];
        }
    }
}