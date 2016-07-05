namespace Anjril.Common.Network.TcpImpl
{
    using Exceptions;
    using global::Common.Logging;
    using Internals;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class TcpSocketClient : ISocketClient
    {
        private static ILog log = LogManager.GetLogger(typeof(TcpSocketClient));

        #region properties

        public bool IsConnected { get; private set; }
        public int Port { get { return (this.Server.TcpClient.Client.LocalEndPoint as IPEndPoint).Port; } }

        #endregion

        #region private properties

        /// <summary>
        /// The server on which this instance is connected
        /// </summary>
        private TcpRemoteConnection Server { get; set; }

        /// <summary>
        /// The delegate executed when a new message is received
        /// </summary>
        private MessageHandler OnMessageReceived { get; set; }

        /// <summary>
        /// Indicates whether this intance has to stop listening
        /// </summary>
        private bool Stop { get; set; }

        #endregion

        #region constructors

        public TcpSocketClient(int port, string separator)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            this.Server = new TcpRemoteConnection(new TcpClient(localEndPoint), separator);
            this.Stop = false;
            this.IsConnected = false;
        }

        #endregion

        #region methods

        public string Connect(string ipAddress, int port, MessageHandler onMessageReceived, string message)
        {
            log.InfoFormat("Connection to {0}:{1} remote connection", ipAddress, port);

            if (this.IsConnected)
            {
                throw new AlreadyConnectedException();
            }

            this.OnMessageReceived = onMessageReceived;

            try
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                this.Server.TcpClient.Connect(remoteEndPoint);
            }
            catch (SocketException e)
            {
                throw new ConnectionFailedException(TypeConnectionFailed.SocketUnreachable, e);
            }

            var msg = new Message(Command.ConnectionRequest, message);

            this.Server.Send(msg);

            Message response = this.ReceiveMessage(1000); // TODO : parameterize timeout

            if (response == null)
            {
                this.ResetConnection();
                throw new ConnectionFailedException();
            }
            else if (!response.IsValid)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(TypeConnectionFailed.InvalidResponse, "The response from the server was not at the expected format. Connection failed");
            }
            else if (response.Command == Command.ConnectionFailed)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(TypeConnectionFailed.ConnectionRefused, response.InnerMessage);
            }
            else if (response.Command != Command.ConnectionGranted)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(TypeConnectionFailed.Other, "An unexpected connection response has been received. Connection failed.");
            }

            var thread = new Thread(new ThreadStart(this.Listening));
            thread.Name = String.Format("TcpSocketClient:{0} Listening", this.GetHashCode());
            thread.Start();

            this.IsConnected = true;

            return response.InnerMessage;
        }

        public void Disconnect(string message)
        {
            this.Disconnect(message, true);
        }

        public void Send(string message)
        {
            var msg = new Message(Command.Message, message);

            try
            {
                this.Server.Send(msg);

                log.DebugFormat("Message sent: {0}", msg);
            }
            catch (SocketException e)
            {
                this.ResetConnection();
                throw new ConnectionLostException(e);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Tries to get a message from the socket during the specified time in milliseconds. If the timeout is reach, returns null
        /// </summary>
        /// <param name="timeout">The timeout</param>
        /// <returns>The message received</returns>
        private Message ReceiveMessage(int timeout)
        {
            var timer = Stopwatch.StartNew();

            while (true)
            {
                var response = this.Server.Receive();

                if (response == null)
                {
                    if (timer.ElapsedMilliseconds > timeout)
                    {
                        timer.Stop();
                        return null;
                    }

                    Thread.Sleep(100); // TODO : parameterize the tick
                }
                else
                {
                    return response;
                }
            }
        }

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private void Listening()
        {
            try
            {
                this.Stop = false;

                log.Debug("The client starts listening for new message");

                while (!this.Stop)
                {
                    var message = this.Server.Receive();

                    if (message == null)
                    {
                        Thread.Sleep(100); // TODO : parameterize the tick
                    }
                    else
                    {
                        if (message.IsValid && message.Command == Command.Message && this.OnMessageReceived != null)
                        {
                            this.OnMessageReceived(this.Server, message.InnerMessage);
                        }
                        else if (message.IsValid && message.Command == Command.Disconnected)
                        {
                            this.ResetConnection();
                            throw new ConnectionLostException(message.InnerMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                log.Debug("The client stops listening for new message");

                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Disconnect the current client from the server, with the specified justification
        /// </summary>
        /// <param name="message">the justification for disconnecting</param>
        /// <param name="reuse">specify if the client is able to connect to another server</param>
        private void Disconnect(string message, bool reuse)
        {
            this.Stop = true;

            Message disconnection = new Message(Command.Disconnection, message);

            this.Server.Send(disconnection);

            // TODO : parameterize timeout
            var response = this.ReceiveMessage(1000);

            while (response != null && response.Command != Command.Disconnected)
            {
                // TODO : parameterize timeout
                response = this.ReceiveMessage(1000);
            }

            if (reuse)
            {
                ResetConnection();
                log.Info("Client disconnected from Server. Can connect again");
            }
            else
            {
                this.Server.TcpClient.Close();
                log.Info("Client disconnected from Server. Can not connect anymore");
            }
        }

        /// <summary>
        /// Closes the connection in a way it can be used again to connect with another server
        /// </summary>
        private void ResetConnection()
        {
            this.Stop = true;
            this.IsConnected = false;

            var ipAddress = IPAddress.Parse("127.0.0.1");
            var port = this.Port;

            this.Server.TcpClient.Close();
            this.Server.TcpClient = new TcpClient(new IPEndPoint(ipAddress, port));
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this.Stop = true;

                if (disposing)
                {
                    if (this.IsConnected)
                    {
                        this.Disconnect(null, false);
                    }
                    else
                    {
                        this.Server.TcpClient.Close();
                    }
                }

                this.OnMessageReceived = null;
                this.Server.TcpClient = null;

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
