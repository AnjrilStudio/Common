using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network
{
    public delegate void ReceiveHandler(RemoteConnection sender, string message);
}
