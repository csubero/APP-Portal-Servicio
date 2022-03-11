using Plugin.Connectivity.Abstractions;
using System;
using System.Threading.Tasks;

namespace PortalServicio.Services
{
    public static class ConnectivityHelper
    {
        public static async Task<bool> AwaitConnected(this IConnectivity connectivity, TimeSpan timeout)
        {
            var end = DateTime.Now + timeout;
            do
            {
                if (connectivity.IsConnected)
                    return true;
                await Task.Delay(1000);
            } while (DateTime.Now < end);
            return false;
        }
    }
}
