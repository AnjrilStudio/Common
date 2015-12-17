using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.State
{
    internal abstract class BaseRequest
    {
        /// <summary>
        /// The targeted remote connection of the request
        /// </summary>
        public IRemoteConnection RemoteConnection { get; set; }

        /// <summary>
        /// The datetime when the request has been shipped
        /// </summary>
        public DateTime ShipmentDate { get; set; }
    }
}
