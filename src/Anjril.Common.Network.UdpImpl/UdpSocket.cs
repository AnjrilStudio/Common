using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpSocket : ISocket
    {
        #region properties

        private IReceiver Receiver { get; set; }
        private ISender Sender { get; set; }

        public int ListeningPort { get { return this.Receiver.ListeningPort; } }

        #endregion

        #region events

        public event MessageHandler OnConnection;
        public event MessageHandler OnConnected;
        public event MessageHandler OnConnectionFailed;
        public event MessageHandler OnReceive;

        #endregion

        #region constructors

        public UdpSocket(int port, MessageHandler onConnection, MessageHandler onConnected, MessageHandler onConnectionFailed, MessageHandler onReceive)
        {
            var udpClient = new UdpClient(port);

            this.Receiver = new UdpReceiver(udpClient, MessageReceived);
            this.Sender = new UdpSender(udpClient);

            this.OnConnection += onConnection;
            this.OnConnected += onConnected;
            this.OnConnectionFailed += onConnectionFailed;
            this.OnReceive += onReceive;
        }

        #endregion

        #region methods

        public void Connect(IRemoteConnection pair)
        {
            throw new NotImplementedException();
        }

        public void StartListening()
        {
            var thread = new Thread(new ThreadStart(this.Receiver.StartListening));
            thread.Start();
        }

        public void Send(string message, IRemoteConnection destination)
        {
            // TODO : 
            // -manage acquittement
            // -manage new connection

            this.Sender.Send(message, destination);
        }

        #endregion

        #region private methods

        private void MessageReceived(IRemoteConnection sender, string message)
        {
            // TODO : manage acquittement
            (sender as UdpRemoteConnection).Sender = this;

            if (this.OnReceive != null)
            {
                this.OnReceive(sender, message);
            }
        }

        #endregion
    }
}
