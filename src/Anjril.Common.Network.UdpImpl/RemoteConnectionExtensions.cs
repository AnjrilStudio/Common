using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    static class RemoteConnectionExtensions
    {
        static IPEndPoint ToIPEndPoint(this RemoteConnection remoteConnection)
        {
            var ipAddress = IPAddress.Parse(remoteConnection.IPAddress);

            return new IPEndPoint(ipAddress, remoteConnection.Port);
        }

        static RemoteConnection ToRemoteConnection(this IPEndPoint endPoint)
        {
            return new RemoteConnection
            {
                IPAddress = endPoint.Address.ToString(),
                Port = endPoint.Port
            };
        }
    }
}
