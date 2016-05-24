using Anjril.Common.Network.Exceptions;
using Anjril.Common.Network.TcpImpl.Internals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Anjril.Common.Network.TcpImpl
{
    public class TcpSocketClient : ISocketClient
    {
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

            Message response = this.ReceiveMessage(1000); // TODO : parameterize timeout

            if (response == null)
            {
                this.Disconnect(null, true);
                throw new ConnectionFailedException();
            }
            else if (!response.IsValid)
            {
                this.Disconnect(null, true);
                throw new ConnectionFailedException(TypeConnectionFailed.InvalidResponse, "The response from the server was not at the expected format. Connection failed");
            }
            else if (response.Command == Command.ConnectionFailed)
            {
                this.Disconnect(null, true);
                throw new ConnectionFailedException(TypeConnectionFailed.ConnectionRefused, response.InnerMessage);
            }
            else if (response.Command != Command.ConnectionGranted)
            {
                this.Disconnect(null, true);
                throw new ConnectionFailedException(TypeConnectionFailed.Other, "An unexpected connection response has been received. Connection failed.");
            }

            var thread = new Thread(new ThreadStart(this.Listening));
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

            this.Server.Send(msg.ToString());
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
                var responseStr = this.Server.Receive();

                if (String.IsNullOrEmpty(responseStr))
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
                    return new Message(responseStr);
                }
            }
        }

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private void Listening()
        {
            this.Stop = false;

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

            this.IsConnected = false;
        }

        /// <summary>
        /// Disconnect the current client from the server, with the specified justification
        /// </summary>
        /// <param name="message">the justification for disconnecting</param>
        /// <param name="reuse">specify if the client is able to connect to another server</param>
        private void Disconnect(string message, bool reuse)
        {
            var port = this.Port;

            this.Stop = true;

            Message disconnection = new Message(Command.Disconnection, message);

            this.Server.Send(disconnection.ToString());

            // TODO : parameterize timeout
            var response = this.ReceiveMessage(1000);

            while (response != null && response.Command != Command.Disconnected)
            {
                // TODO : parameterize timeout
                response = this.ReceiveMessage(1000);
            }

            this.Server.TcpClient.Close();

            if (reuse)
            {
                var ipAddress = IPAddress.Parse("127.0.0.1");

                this.Server.TcpClient.Close();
                this.Server.TcpClient = new TcpClient(new IPEndPoint(ipAddress, port));
            }
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
