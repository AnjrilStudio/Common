using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpSender : ISender
    {
        #region properties

        private UdpClient Sender { get; set; }

        #endregion

        #region constructors

        public UdpSender(UdpClient udpClient)
        {
            this.Sender = udpClient;
        }

        #endregion

        #region methods

        public void Send(string message, IRemoteConnection destination)
        {
            var datagram = Encoding.ASCII.GetBytes(message);

            this.Sender.Send(datagram, datagram.Length, new IPEndPoint(IPAddress.Parse(destination.IPAddress), destination.Port));
        }

        #endregion
    }
}
