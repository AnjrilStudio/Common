using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    internal static class RemoteConnectionExtensions
    {
        internal static UdpRemoteConnection ToUdpRemoteConnection(this IRemoteConnection remoteConnection)
        {
            return remoteConnection as UdpRemoteConnection;
        }
    }
}
