namespace Anjril.Common.Network.UdpImpl
{
    using Anjril.Common.Network.UdpImpl.Internal;
    using Exceptions;
    using System;
    using System.Net.Sockets;
    using System.Threading;
    public class UdpSocketClient : ISocketClient
    {
        #region properties

        public bool IsConnected{ get; private set; }
        public int Port { get { return this.SocketHelper.Port; } }

        #endregion

        #region internal properties

        internal ISocketHelper SocketHelper { get; set; }
        internal UdpRemoteConnection RemoteConnection { get; set; }
        internal Nullable<ConnectionResultCode> ConnectionCode { get; private set; }

        #endregion

        #region events

        public event MessageHandler OnReceive;

        #endregion

        #region constructors

        public UdpSocketClient(int port)
        {
            this.SocketHelper = new BasicUdpSocketHelper(new UdpClient(port), OnMessageReceived);
        }

        #endregion

        #region methods

        public void Connect(IRemoteConnection pair, string message)
        {
            if (this.IsConnected)
            {
                throw new ConnectionFailedException(ConnectionResultCode.ALREADY_CONNECTED);
            }

            this.SocketHelper.StartListening();

            var connectionRequest = new Message(0, Command.Connect, message);

            for(int i = 0; i < 5; i++)
            {
                // TODO : parameterize the timeout
                Thread.Sleep(200);

                if (this.ConnectionCode.HasValue && this.ConnectionCode == ConnectionResultCode.OK)
                {
                    this.IsConnected = true;
                    break;
                }
                else if (this.ConnectionCode.HasValue)
                {
                    throw new ConnectionFailedException(this.ConnectionCode.Value);
                }
            }
        }

        public void Send(string message)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var msg = new Message(this.RemoteConnection.NextSendingId, Command.Other, message);
            this.RemoteConnection.IncrementSendingId();
            this.SocketHelper.Send(msg, this.RemoteConnection);
        }

        #endregion

        #region private methods

        private void OnMessageReceived(UdpRemoteConnection sender, Message message)
        {
            if (sender.Equals(this.RemoteConnection) && message.IsValid)
            {
                switch (message.Command)
                {
                    case Command.ConnectionGranted:
                        this.IsConnected = true;
                        break;
                    case Command.Other:
                        var handler = this.OnReceive;

                        if(handler != null)
                        {
                            handler(sender, message.InnerMessage);
                        }
                        break;
                    default:
                        Console.WriteLine("An unexpected message was received: " + message.ToString());
                        break;
                }
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.SocketHelper.Dispose();
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
