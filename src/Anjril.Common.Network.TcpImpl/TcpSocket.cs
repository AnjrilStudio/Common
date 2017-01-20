namespace Anjril.Common.Network.TcpImpl
{
    using Exceptions;
    using Internals;
    using Logging;
    //using Properties;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class TcpSocket : ISocket
    {
        #region fields

        private AnjrilLogger logger = AnjrilNetworkLogging.CreateLogger<TcpRemoteConnection>();

        #endregion

        #region properties

        public bool IsListening { get; private set; }
        public int Port { get { return (this.Listener.LocalEndpoint as IPEndPoint).Port; } }
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

        public TcpSocket()
        {
            this.Listener = new TcpListener(IPAddress.Any, Settings.Default.ServerPort);
            this.Clients = new List<IRemoteConnection>();
            this.Stop = false;
        }

        #endregion

        #region methods

        public void Broadcast(string message)
        {
            foreach (var remote in this.Clients.ToList())
            {
                remote.Send(message);
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
            if (this.Clients.Contains(client))
            {
                var tcpRemote = (client as TcpRemoteConnection);

                tcpRemote.Send(new Message(Command.Disconnected, justification));

                this.Clients.Remove(client);

                tcpRemote.TcpClient.Dispose();
            }
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

                    var remote = new TcpRemoteConnection(client, this);

                    var thread = new Thread(new ParameterizedThreadStart(this.ValidateConnection));
                    thread.Name = String.Format("TcpSocket:{0} ValidateConnection", this.GetHashCode());
                    thread.Start(remote);
                }
                catch (SocketException e)
                {
                    // Handle exceptions that needs to be thrown and those that don't
                    switch (e.SocketErrorCode)
                    {
                        case SocketError.Interrupted: // the tcp listener has been stopped, we don't need to throw an exception in that case
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

            logger.LogNetwork($"Validating the connection of {remote.IPAddress}:{remote.Port}");

            try
            {
                Message request = null;

                var connectionTimeout = Settings.Default.ConnectionTimeout;

                var chrono = Stopwatch.StartNew();

                while (request == null)
                {
                    if (chrono.ElapsedMilliseconds > connectionTimeout)
                    {
                        Message response = new Message(Command.ConnectionFailed, "The connection request takes to long.");

                        logger.LogNetwork("Connection Timeout");

                        remote.Send(response);
                        remote.TcpClient.Close();
                        return;
                    }

                    request = remote.Receive();

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

                    remote.Send(response);

                    if (success)
                    {
                        this.Clients.Add(remote);
                        logger.LogNetwork("Connection Granted");
                    }
                    else
                    {
                        remote.TcpClient.Close();
                        logger.LogNetwork("Connection Refused");
                    }
                }
                else
                {
                    Message response = new Message(Command.ConnectionFailed, "The connection request is not in the valid format or with the expected command.");

                    remote.Send(response);
                    remote.TcpClient.Close();

                    logger.LogNetwork("Connection Request Invalid");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            finally
            {
                logger.LogNetwork($"Validation of {remote.IPAddress}:{remote.Port} finished");
            }
        }

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private void ListeningMessage()
        {
            try
            {
                logger.LogNetwork("The server starts listening for new message");

                var listeningTick = Settings.Default.ListeningTick;

                while (!this.Stop)
                {
                    var disconnectedRemote = new List<TcpRemoteConnection>();

                    foreach (var remote in this.Clients.Cast<TcpRemoteConnection>())
                    {
                        for (var message = remote.Receive(); message != null; message = remote.Receive())
                        {
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

                    Thread.Sleep(listeningTick);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            finally
            {
                this.IsListening = false;
                logger.LogNetwork("The Server stops listenning for new message");
            }
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
