using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpReceiver : IReceiver
    {
        #region properties

        public int ListeningPort { get { return (this.Listener.Client.LocalEndPoint as IPEndPoint).Port; } }

        private UdpClient Listener { get; set; }

        #endregion

        #region events

        public event ReceiveHandler OnReceive;

        #endregion

        #region contructors

        public UdpReceiver(UdpClient udpClient)
        {
            this.Listener = udpClient;
        }

        #endregion

        #region methods

        public void StartListening()
        {
            while(true) // TODO : use a variable to be able to stop 
            {
                // Get datagram
                var datagram = this.Listener.ReceiveAsync();

                // Wait until the datagram is received
                datagram.Wait();

                // Decode datagram
                var message = Encoding.ASCII.GetString(datagram.Result.Buffer);

                // Raise OnReceive event
                if (this.OnReceive != null)
                {
                    var remoteConnection = new RemoteConnection { Port = datagram.Result.RemoteEndPoint.Port, IPAddress = datagram.Result.RemoteEndPoint.Address.ToString() };

                    this.OnReceive(remoteConnection, message);
                }
            }
        }
        
        #endregion
    }
}
