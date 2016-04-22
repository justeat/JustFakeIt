using System;
using System.Net;
using System.Net.NetworkInformation;

namespace JustFakeIt
{
    public class Ports
    {
        private static readonly Random Random = new Random();

        public static readonly object FreePortCreationLock = new object();

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
            var allActiveTcpConnections = ipGlobalProperties.GetActiveTcpConnections();
            var activeTcpConnections = allActiveTcpConnections.GetEnumerator();

            while (activeTcpConnections.MoveNext())
            {
                var tcpConnectionInformation = (TcpConnectionInformation) activeTcpConnections.Current;
                if (tcpConnectionInformation.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }

            return true;
        }
    }
}