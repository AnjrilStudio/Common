using Anjril.Common.Network.UdpImpl.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpSocket : ISocket
    {
        public bool IsListening
        {
            get
            {
                return this.Helper.IsListening;
            }
        }

        public int Port
        {
            get
            {
                return this.Helper.Port;
            }
        }

        private UdpSenderReceiver Helper { get; set; }

        private IList<IRemoteConnection> RemoteConnected { get; set; }

        public event MessageHandler OnConnection;
        public event MessageHandler OnReceive;

        public UdpSocket(int port)
        {
            this.Helper = new UdpSenderReceiver(new UdpClient(port), OnMessageReceived);

            this.RemoteConnected = new List<IRemoteConnection>();
        }

        private void OnMessageReceived(UdpRemoteConnection sender, Message message)
        {
            switch(message.Command)
            {
                case Command.Connect:
                    if (this.RemoteConnected.Contains(sender))
                    {
                        // TODO : calculate unique Id
                        Message connectionFailed = new Message(0, Command.AlreadyConnected, String.Empty);
                        sender.Send(connectionFailed);
                    }
                    // TODO
                    break;
                case Command.Other:
                    var handler = this.OnReceive;
                    if(handler != null)
                    {
                        handler(sender, message.InnerMessage);
                    }
                    break;
            }
        }

        public void Broadcast(string message)
        {
            foreach(IRemoteConnection remote in this.RemoteConnected)
            {
                remote.Send(message);
            }
        }

        public void StartListening()
        {
            this.Helper.StartListening();
        }

        public void StopListening()
        {
            this.StopListening();
        }

        #region IDisposable Support

        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Helper.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        #endregion
    }
}
