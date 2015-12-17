using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.State
{
    internal class ConnectionRequest : BaseRequest
    {
        /// <summary>
        /// The acquittal of the connection resquest sended to the remote connection
        /// </summary>
        public AcquittalRequest Message { get; set; }
    }
}
