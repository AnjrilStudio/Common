using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    internal enum Command
    {
        Acquittal,
        Connect,
        ConnectionGranted,
        ConnectionRefused,
        ConnectionNeeded,
        Ping,
        Pong,
        Other
    }
}
