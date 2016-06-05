namespace Anjril.Common.Network.UdpImpl
{
    using Anjril.Common.Network.UdpImpl.Internal;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;

    public class UdpSocket : ISocket
    {
        #region properties

        public bool IsListening { get { return this.SocketHelper.IsListening; } }
        public int Port { get { return this.SocketHelper.Port; } }

        #endregion

        #region internal properties

        internal ISocketHelper SocketHelper { get; set; }
        internal IList<UdpRemoteConnection> RemotesConnected { get; set; }

        #endregion

        #region events

        public event ConnectionHandler OnConnection;
        public event MessageHandler OnReceive;

        #endregion

        #region constructors

        public UdpSocket(int port)
        {
            this.SocketHelper = new BasicUdpSocketHelper(new UdpClient(port), OnMessageReceived);
            this.RemotesConnected = new List<UdpRemoteConnection>();
        }

        #endregion

        #region methods

        public void Broadcast(string message)
        {
            foreach(UdpRemoteConnection remote in this.RemotesConnected)
            {
                remote.Send(message);
            }
        }

        public void StartListening()
        {
            this.SocketHelper.StartListening();
        }

        public void StopListening()
        {
            this.SocketHelper.StopListening();
        }

        #endregion

        #region private methods

        private void OnMessageReceived(UdpRemoteConnection sender, Message message)
        {
            if (message.IsValid)
            {
                var isConnected = this.RemotesConnected.Contains(sender);

                switch (message.Command)
                {
                    case Command.Connect:
                        ManageConnectionRequest(sender, message, isConnected);
                        break;
                    case Command.Other:
                        ManageRegularMessage(sender, message, isConnected);
                        break;
                    default:
                        Console.WriteLine("An unexpected message was received: " + message.ToString());
                        break;
                }
            }
        }

        /// <summary>
        /// Method calls when a regular message arrives. Determines if it fires the message handler or sends back a connection needed message.
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="message">The message</param>
        /// <param name="isConnected">Indicates whether the sender is connected</param>
        internal void ManageRegularMessage(UdpRemoteConnection sender, Message message, bool isConnected)
        {
            if (!isConnected)
            {
                Message response = new Message(sender.NextSendingId, Command.ConnectionNeeded, String.Empty);
                sender.IncrementSendingId();
                sender.Send(response);
            }
            else
            {
                var messageHandler = this.OnReceive;
                if (messageHandler != null)
                {
                    messageHandler(sender, message.InnerMessage);
                }

            }
        }

        /// <summary>
        /// Method calls when a connection request arrives. Determines if it add the sender into the connected list or refused the connection.
        /// </summary>
        /// <param name="sender">The sender of the request</param>
        /// <param name="message">The request</param>
        /// <param name="isConnected">Indicates whether the sender is connected</param>
        internal void ManageConnectionRequest(UdpRemoteConnection sender, Message message, bool isConnected)
        {
            Message response;

            if (isConnected)
            {
                response = new Message(sender.NextSendingId, Command.AlreadyConnected, String.Empty);
            }
            else
            {
                var connectionHandler = this.OnConnection;

                if (connectionHandler != null)
                {
                    string responseStr;
                    var success = connectionHandler(sender, message.InnerMessage, out responseStr);

                    if(success)
                    {
                        this.RemotesConnected.Add(sender);
                    }

                    response = success ? new Message(sender.NextSendingId, Command.ConnectionGranted, responseStr) : new Message(0, Command.ConnectionRefused, responseStr);
                }
                else
                {
                    this.RemotesConnected.Add(sender);
                    response = new Message(sender.NextSendingId, Command.ConnectionGranted, String.Empty);
                }
            }

            sender.IncrementSendingId();
            sender.Send(response);
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
