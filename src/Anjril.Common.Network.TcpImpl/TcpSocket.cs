namespace Anjril.Common.Network.TcpImpl
{
    using Exceptions;
    using Internals;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class TcpSocket : ISocket
    {
        #region properties

        public bool IsListening { get; private set; }
        public int Port { get { return (this.Listener.Server.LocalEndPoint as IPEndPoint).Port; } }
        public IList<IRemoteConnection> Clients { get; private set; }

        #endregion

        #region private properties

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
            this.Clients = new List<IRemoteConnection>();
            this.Separator = separator;
            this.Stop = false;
        }

        #endregion

        #region methods

        public void Broadcast(string message)
        {
            var disconnectedRemote = new List<TcpRemoteConnection>();

            foreach (var remote in this.Clients)
            {
                try
                {
                    remote.Send(message);
                }
                catch (SocketException e)
                {
                    switch (e.ErrorCode)
                    {
                        case 10053:
                            // The remote connection is disconnected
                            disconnectedRemote.Add(remote as TcpRemoteConnection);
                            break;
                        default:
                            // In all the other cases, the exception is thrown
                            throw e;
                    }
                }
            }

            foreach (var remote in disconnectedRemote)
            {
                this.RemoveClient(remote);
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
            thread.Name = String.Format("TcpSocket:{0} ListeningMessage", this.GetHashCode());
            thread.Start();

            thread = new Thread(new ThreadStart(this.ListeningConnection));
            thread.Name = String.Format("TcpSocket:{0} ListeningConnection", this.GetHashCode());
            thread.Start();
        }

        public void StopListening()
        {
            this.Listener.Stop();
            this.Stop = true;
            this.ResetConnections();
        }

        public void CloseConnection(IRemoteConnection client, string justification)
        {
            if (this.Clients.Contains(client)) { }
            var tcpRemote = (client as TcpRemoteConnection);

            tcpRemote.Send(new Message(Command.Disconnected, justification));
            tcpRemote.TcpClient.Close();

            this.Clients.Remove(client);
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
                    thread.Name = String.Format("TcpSocket:{0} ValidateConnection", this.GetHashCode());
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
                if (i > 10) // TODO : parameterize the timeout
                {
                    Message response = new Message(Command.ConnectionFailed, "The connection request takes to long.");

                    remote.Send(response);
                    remote.TcpClient.Close();
                    return;
                }

                var requestStr = remote.Receive();

                if (requestStr == null)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    request = new Message(requestStr);
                }
            }

            if (request.IsValid && request.Command == Command.ConnectionRequest)
            {
                string responseStr = null;
                var success = this.OnConnectionRequested != null
                    ? this.OnConnectionRequested(remote, request.InnerMessage, out responseStr)
                    : true;

                Message response = new Message(success ? Command.ConnectionGranted : Command.ConnectionFailed, responseStr);

                remote.Send(response);

                if (success)
                {
                    this.Clients.Add(remote);
                }
                else
                {
                    remote.TcpClient.Close();
                }
            }
            else
            {
                Message response = new Message(Command.ConnectionFailed, "The connection request is not in the valid format or with the expected command.");

                remote.Send(response);
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
                var disconnectedRemote = new List<TcpRemoteConnection>();

                foreach (var remote in this.Clients.Cast<TcpRemoteConnection>())
                {
                    for (var messageStr = remote.Receive(); !String.IsNullOrEmpty(messageStr); messageStr = remote.Receive())
                    {
                        var message = new Message(messageStr);

                        if (message.IsValid && message.Command == Command.Message)
                        {
                            this.OnMessageReceived?.Invoke(remote, message.InnerMessage);
                        }
                        else if (message.IsValid && message.Command == Command.Disconnection)
                        {
                            disconnectedRemote.Add(remote);

                            this.OnDisconnect?.Invoke(remote, message.InnerMessage);
                        }
                    }
                }

                foreach (var remote in disconnectedRemote)
                {
                    remote.Send(new Message(Command.Disconnected, null));
                    this.RemoveClient(remote);
                }

                Thread.Sleep(100); // TODO : parameterize tick
            }

            this.IsListening = false;
        }

        /// <summary>
        /// Removes the specified client from the list of <see cref="Clients"/>. Also disposes the underlying TcpClient instance.
        /// </summary>
        /// <param name="client">The client to remove</param>
        private void RemoveClient(TcpRemoteConnection client)
        {
            this.Clients.Remove(client);
            client.TcpClient.Close();
        }

        /// <summary>
        /// Close all the remote connected and clear the list
        /// </summary>
        private void ResetConnections()
        {
            foreach (var remote in this.Clients.Cast<TcpRemoteConnection>())
            {
                remote.TcpClient.Close();
            }

            this.Clients = new List<IRemoteConnection>();
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
