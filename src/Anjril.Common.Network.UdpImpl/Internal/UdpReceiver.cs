namespace Anjril.Common.Network.UdpImpl.Internal
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    internal class UdpReceiver : IReceiver
    {
        #region properties

        public int Port { get { return (this.Listener.Client.LocalEndPoint as IPEndPoint).Port; } }
        public bool IsListening { get; private set; }

        #endregion

        #region private properties

        /// <summary>
        /// The <see cref="UdpClient"/> used to receive messages
        /// </summary>
        private UdpClient Listener { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the receiver has to stop listening
        /// </summary>
        private bool Stop { get; set; }

        /// <summary>
        /// The <see cref="UdpHelper"/> used to manage message
        /// </summary>
        public UdpHelper UdpHelper { get; set; }

        #endregion

        #region events

        public event InternalMessageHandler OnReceive;

        #endregion

        #region contructors

        public UdpReceiver(UdpClient udpClient, InternalMessageHandler handler)
        {
            this.Listener = udpClient;
            this.UdpHelper = new UdpHelper(udpClient);

            this.IsListening = false;
            this.Stop = false;

            this.OnReceive += handler;
        }

        #endregion

        #region methods

        public void StartListening()
        {
            // TODO : how to handle this ???
            //if(this.IsListening)
            //{
            //    throw new AlreadyListeningException(this);
            //}

            var thread = new Thread(new ThreadStart(this.Listening));
            thread.Start();
        }

        public void StopListening()
        {
            this.Stop = true;
            this.Listener.Close();
        }

        #endregion

        #region private methods

        private void Listening()
        {
            this.IsListening = true;

            while (!this.Stop)
            {
                // Create a default endPoint that will be updated with the next message endpoint properties
                var endPoint = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    // Get datagram
                    var datagram = this.Listener.Receive(ref endPoint);

                    // Decode datagram
                    var message = this.UdpHelper.DeserializeDatagram(datagram);

                    // Raise OnReceive event
                    if (this.OnReceive != null)
                    {
                        var remoteConnection = new UdpRemoteConnection(endPoint);

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
                                this.OnMessageReceived(new Message(message), remoteConnection);
                            }));
                        }
                        else
                        {
#endif
                        this.OnMessageReceived(new Message(message), remoteConnection);
#if WAN
                        }
#endif
                    }
                }
                catch (SocketException e)
                {
                    switch (e.NativeErrorCode)
                    {
                        case 10004: // the listener has been closed
                        //case XXX: // manage other specifics exception that don't need to be thrown 
                            break; 
                        default:
                            throw e;
                    }
                }
            }

            this.IsListening = false;
            this.Stop = false;
        }

        private void OnMessageReceived(Message message, UdpRemoteConnection remoteConnection)
        {
            if (message.IsValid)
            {
                if (message.Command != Command.Acquittal)
                {
                    var response = new Message(message.Id, Command.Acquittal, null);
                    this.UdpHelper.SendMessage(response, remoteConnection);
                }

                this.OnReceive(remoteConnection, message);
            }
        }

        #endregion

        #region IDisposable support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (IsListening)
                        this.StopListening();
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
