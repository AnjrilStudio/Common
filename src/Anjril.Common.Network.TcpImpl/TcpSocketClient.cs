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
    public class TcpSocketClient : ISocketClient
    {
        #region private fields

        /// <summary>
        /// A lock used to ensure the listening thread is not using the underlying socket when it is disposed
        /// </summary>
        private readonly object disposeLock = new Object();

        #endregion

        #region properties

        public bool IsConnected
        {
            get
            {
                if (this.Server.TcpClient.Client != null)
                {
                    return this.Server.TcpClient.Connected;
                }
                return false;
            }
        }
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
        }

        #endregion

        #region methods

        public string Connect(string ipAddress, int port, MessageHandler onMessageReceived, string message)
        {
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

            this.Server.Send(msg.ToString());

            Message response = null;

            for (int i = 0; response == null; i++)
            {
                var responseStr = this.Server.Receive();

                if (String.IsNullOrEmpty(responseStr))
                {
                    if (i > 10) // TODO : parameterize the timeout
                    {
                        this.ResetConnection();
                        throw new ConnectionFailedException();
                    }

                    Thread.Sleep(100);
                }
                else
                {
                    response = new Message(responseStr);
                }
            }

            if (!response.IsValid)
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
            else
            {
                if (this.OnMessageReceived != null)
                {
                    this.OnMessageReceived(this.Server, response.InnerMessage);
                }

                var thread = new Thread(new ThreadStart(this.Listening));
                thread.Start();
            }

            return response.InnerMessage;
        }

        public void Disconnect(string message)
        {
            Message disconnection = new Message(Command.Disconnection, message);

            this.Server.Send(disconnection.ToString());

            this.ResetConnection();
        }

        public void Send(string message)
        {
            var msg = new Message(Command.Message, message);

            this.Server.Send(msg.ToString());
        }

        #endregion

        #region private methods

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private void Listening()
        {
            while (!this.Stop)
            {
                string messageStr = this.Server.Receive();

                if (String.IsNullOrEmpty(messageStr))
                {
                    Thread.Sleep(100); // TODO : parameterize the tick
                }
                else
                {
                    var message = new Message(messageStr);

                    if (message.IsValid && message.Command == Command.Message && this.OnMessageReceived != null)
                    {
                        this.OnMessageReceived(this.Server, message.InnerMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the connection, to be able to connect again to another server
        /// </summary>
        private void ResetConnection()
        {
            var port = this.Port;
            var ipAddress = IPAddress.Parse("127.0.0.1");

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
                    this.Server.TcpClient.Close();
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
