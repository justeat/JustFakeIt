using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace JustFakeIt
{
    public class Ports
    {
        private static readonly Random Random = new Random();

        public static int GetFreeTcpPort()
        {
            var port = 0;
            while (!IsAvailable(port))
                port = Random.Next(49152, IPEndPoint.MaxPort);

            return port;
        }

        private static bool IsAvailable(int port)
        {
            if (port == 0)
            {
                return false;
            }

            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = ipGlobalProperties.GetActiveTcpConnections();
            var activeTcpListeners = ipGlobalProperties.GetActiveTcpListeners();

            return activeTcpConnections.All(x => x.LocalEndPoint.Port != port) &&
                   activeTcpListeners.All(x => x.Port != port);
        }
    }
}