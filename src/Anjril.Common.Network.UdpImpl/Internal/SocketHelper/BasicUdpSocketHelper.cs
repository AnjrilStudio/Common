namespace Anjril.Common.Network.UdpImpl.Internal
{
    using Exceptions;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// This class is a basic implementation of the ISocketHelper, which only sends and receives messages, without acknowledgment and order delivery.
    /// </summary>
    internal class BasicUdpSocketHelper : ISocketHelper
    {
        #region properties

        public bool IsListening { get; protected set; }
        public int Port { get { return this.UdpHelper.Port; } }

        #endregion

        #region protected properties

        /// <summary>
        /// UdpHelper used internally to send messages
        /// </summary>
        protected UdpHelper UdpHelper { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the receiver has to stop listening
        /// </summary>
        protected bool Stop { get; set; }

        #endregion

        #region events

        public event InternalMessageHandler OnReceive;

        #endregion

        #region constructors

        public BasicUdpSocketHelper(UdpClient client, InternalMessageHandler onReceived)
        {
            this.UdpHelper = new UdpHelper(client);
            this.OnReceive += onReceived;
        }

        #endregion

        #region methods

        public void Send(Message message, UdpRemoteConnection destination)
        {
            this.UdpHelper.SendMessage(message.ToString(), destination.EndPoint);
        }

        public void StartListening()
        {
            if (this.IsListening)
            {
                throw new SocketHelperAlreadyListeningException(this);
            }

            var thread = new Thread(new ThreadStart(this.Listening));
            thread.Start();
        }

        public void StopListening()
        {
            this.Stop = true;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Listens for a message and when it arrives, fire the <see cref="OnReceive"/> event
        /// </summary>
        private void Listening()
        {
            this.IsListening = true;
            this.UdpHelper.Flush();

            while (!this.Stop)
            {
                IPEndPoint endpoint;

                var messageStr = this.UdpHelper.ReceiveMessage(out endpoint);

                var handler = this.OnReceive;
                if (handler != null)
                {
                    var remote = new UdpRemoteConnection(endpoint, this);
                    var message = new Message(messageStr);

#if WAN
                    // In WAN mode, we simulate the disorder of packages arrival
                    // 1 package in 3 will be effectively received 500ms after its true arrival
                    var random = new Random();

                    if (random.Next(3) == 0)
                    {
                        Console.WriteLine(@"/!\ Package Delayed /!\");

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(500);
                            handler(remote, message);
                        }));
                    }
                    else
                    {
#endif
                        handler(remote, message);
#if WAN
                    }
#endif
                }
            }

            this.IsListening = false;
            this.Stop = false;
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
                    this.UdpHelper.Dispose();
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
