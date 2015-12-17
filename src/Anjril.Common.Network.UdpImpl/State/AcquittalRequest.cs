using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.State
{
    internal class AcquittalRequest : BaseRequest
    {
        /// <summary>
        /// The message sended to the remote connection
        /// </summary>
        public Message Message { get; set; }
    }
}
