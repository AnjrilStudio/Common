using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.TcpImpl.Internals
{
    enum Command
    {
        ConnectionRequest,
        ConnectionGranted,
        ConnectionFailed,
        Message,
        Disconnection,
        Disconnected
    }
}
