using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Anjril.Common.Network
{
    public delegate void MessageHandler(IRemoteConnection sender, string message);
}
