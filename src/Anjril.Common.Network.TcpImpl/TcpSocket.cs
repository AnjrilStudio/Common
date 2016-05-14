using Anjril.Common.Network.Exceptions;
using Anjril.Common.Network.TcpImpl.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Anjril.Common.Network.TcpImpl
{
    public class TcpSocket : ISocket
    {
        #region properties

        public bool IsListening { get; private set; }
        public int Port { get { return (this.Listener.Server.LocalEndPoint as IPEndPoint).Port; } }

        #endregion

        #region private properties

        /// <summary>
        /// The list of clients successfully connected to this socket
        /// </summary>
        internal IList<TcpRemoteConnection> RemoteConnections { get; private set; }

        /// <summary>
        /// THe internal tcp listener used to exchange package
        /// </summary>
        private TcpListener Listener { get; set; }

        /// <summary>
        /// Indicates whether this intance has to stop listening
        /// </summary>
        private bool Stop { get; set; }

        /// <summary>
        /// The separator used to distinguish different message received
        /// </summary>
        private string Separator { get; set; }

        /// <summary>
        /// The delegate executed when a connection request arrives
        /// </summary>
        private ConnectionHandler OnConnectionRequested { get; set; }

        /// <summary>
        /// The delegate executed when a new message is received
        /// </summary>
        private MessageHandler OnMessageReceived { get; set; }

        /// <summary>
        /// The delegate executed when a remote is disconnected
        /// </summary>
        public DisconnectionHandler OnDisconnect { get; private set; }

        #endregion

        #region constructors

        public TcpSocket(int port, string separator)
        {
            this.Listener = new TcpListener(IPAddress.Any, port);
            this.RemoteConnections = new List<TcpRemoteConnection>();
            this.Separator = separator;
            this.Stop = false;
        }

        #endregion

        #region methods

        public void Broadcast(string message)
        {
            foreach (var remote in this.RemoteConnections)
            {
                Message msg = new Message(Command.Message, message);
                remote.Send(msg.ToString());
            }
        }

        public void StartListening(ConnectionHandler onConnectionRequested, MessageHandler onMessageReceived, DisconnectionHandler onDisconnect)
        {
            if (this.IsListening)
            {
                throw new AlreadyListeningException();
            }

            this.OnConnectionRequested = onConnectionRequested;
            this.OnMessageReceived = onMessageReceived;
            this.OnDisconnect = onDisconnect;

            var thread = new Thread(new ThreadStart(this.ListeningMessage));
            thread.Start();

            thread = new Thread(new ThreadStart(this.ListeningConnection));
            thread.Start();
        }

        public void StopListening()
        {
            this.Listener.Stop();
            this.Stop = true;
            this.ResetConnections();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Listens for new connection request
        /// </summary>
        private void ListeningConnection()
        {
            this.Listener.Start();
            this.IsListening = true;

            while (!this.Stop)
            {
                try
                {
                    var client = this.Listener.AcceptTcpClient();

                    var remote = new TcpRemoteConnection(client, this.Separator);

                    var thread = new Thread(new ParameterizedThreadStart(this.ValidateConnection));
                    thread.Start(remote);
                }
                catch (SocketException e)
                {
                    // Handle exceptions that needs to be thrown and those that don't
                    switch (e.NativeErrorCode)
                    {
                        case 10004: // the tcp listener has been stopped, we don't need to throw an exception in that case
                                    //case XXX: // manage other specifics exception that don't need to be thrown 
                            break;
                        default:
                            throw e;
                    }
                }
            }

            this.IsListening = false;
        }

        /// <summary>
        /// Wait for the connection request, and execute the <see cref="OnConnectionRequested"/> delegate when it arrives
        /// </summary>
        /// <param name="remoteConnection"></param>
        private void ValidateConnection(Object remoteConnection)
        {
            var remote = (TcpRemoteConnection)remoteConnection;

            Message request = null;

            for (int i = 0; request == null; i++)
            {
                var requestStr = remote.Receive();
                request = new Message(requestStr);

                if (i > 10) // TODO : parameterize the timeout
                {
                    Message response = new Message(Command.ConnectionFailed, "The connection request takes to long to arrive.");

                    remote.Send(response.ToString());
                    remote.TcpClient.Close();
                    return;
                }

                if (request == null)
                {
                    Thread.Sleep(100);
                }
            }

            if (request.IsValid && request.Command == Command.ConnectionRequest)
            {
                string responseStr = null;
                var success = this.OnConnectionRequested != null
                    ? this.OnConnectionRequested(remote, request.InnerMessage, out responseStr)
                    : true;

                Message response = new Message(success ? Command.ConnectionGranted : Command.ConnectionFailed, responseStr);

                remote.Send(response.ToString());

                if (success)
                {
                    this.RemoteConnections.Add(remote);
                }
                else
                {
                    remote.TcpClient.Close();
                }
            }
            else
            {
                Message response = new Message(Command.ConnectionFailed, "The connection request is not in the valid format or with the expected command.");

                remote.Send(response.ToString());
                remote.TcpClient.Close();
            }
        }

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private void ListeningMessage()
        {
            while (!this.Stop)
            {
                var disconnectedRemote = new LinkedList<TcpRemoteConnection>();

                foreach (var remote in this.RemoteConnections)
                {
                    if (remote.TcpClient.Connected)
                    {
                        for (var messageStr = remote.Receive(); !String.IsNullOrEmpty(messageStr); messageStr = remote.Receive())
                        {
                            var message = new Message(messageStr);

                            if (message.IsValid && message.Command == Command.Message && this.OnMessageReceived != null)
                            {
                                this.OnMessageReceived(remote, message.InnerMessage);
                            }
                            else if (message.IsValid && message.Command == Command.Disconnection && this.OnDisconnect != null)
                            {
                                disconnectedRemote.AddLast(remote);

                                this.OnDisconnect(remote, message.InnerMessage);
                            }
                        }
                    }
                    else
                    {
                        disconnectedRemote.AddLast(remote);
                    }
                }

                foreach (var remote in disconnectedRemote)
                {
                    this.RemoteConnections.Remove(remote);
                }

                Thread.Sleep(100); // TODO : parameterize tick
            }

            this.IsListening = false;
        }

        /// <summary>
        /// Close all the remote connected and clear the list
        /// </summary>
        private void ResetConnections()
        {
            foreach (var remote in this.RemoteConnections)
            {
                remote.TcpClient.Close();
            }

            this.RemoteConnections = new List<TcpRemoteConnection>();
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
                    this.StopListening();

                    ResetConnections();
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
