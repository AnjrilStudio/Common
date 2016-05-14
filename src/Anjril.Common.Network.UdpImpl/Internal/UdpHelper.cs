namespace Anjril.Common.Network.UdpImpl.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    internal class UdpHelper : IDisposable
    {
        #region properties

        /// <summary>
        /// The port used to send and receive messages
        /// </summary>
        public int Port { get { return (this.UdpClient.Client.LocalEndPoint as IPEndPoint).Port; } }

        /// <summary>
        /// The udp client used within this helper to receive and send messages.
        /// </summary>
        protected UdpClient UdpClient { get; set; }

        #endregion

        #region constructors

        public UdpHelper(UdpClient udpClient)
        {
            this.UdpClient = udpClient;
        }

        #endregion

        #region methods

        /// <summary>
        /// Clear the pending message on the port.
        /// </summary>
        public void Flush()
        {
            // TODO
        }

        /// <summary>
        /// Blocks the current thread by waiting for an udp datagram
        /// </summary>
        /// <param name="sender">The sender of the received message</param>
        /// <returns>The message receveived</returns>
        public string ReceiveMessage(out IPEndPoint sender)
        {
            string message;
            sender = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                var datagram = this.UdpClient.Receive(ref sender);
                message = this.DeserializeDatagram(datagram);
            }
            catch (SocketException e)
            {
                // Handle exceptions that needs to be thrown and those that does not
                switch (e.NativeErrorCode)
                {
                    case 10004: // the udp client has been disposed, we don't need to throw an exception in that case
                    //case XXX: // manage other specifics exception that don't need to be thrown 
                        break;
                    default:
                        throw e;
                }

                message = null;
                sender = null;
            }

            return message;
        }

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        public void SendMessage(string message, IPEndPoint destination)
        {
#if WAN
            // In WAN mode, we simulate the lost of packages
            // 1 package in 5 is lost

            var random = new Random();

            if (random.Next(5) == 0)
            {
                Console.WriteLine(@"/!\ Package lost /!\");
            }
            else
            {
#endif
                var datagram = this.SerializeMessage(message);
                this.UdpClient.Send(datagram, datagram.Length, destination);
#if WAN
            }
#endif
        }

        /// <summary>
        /// Serializes a string into a byte array
        /// </summary>
        /// <param name="message">The string to serialize</param>
        /// <returns>The string serialized into a byte array</returns>
        public byte[] SerializeMessage(string message)
        {
            // TODO : parameterize the default encoding
            var datagram = Encoding.ASCII.GetBytes(message);

            return datagram;
        }

        /// <summary>
        /// Deserializes a byte array into a string
        /// </summary>
        /// <param name="datagram">The byte array to deserialize</param>
        /// <returns>The string deserialized</returns>
        public string DeserializeDatagram(byte[] datagram)
        {
            // TODO : parameterize the default encoding
            var message = Encoding.ASCII.GetString(datagram);

            return message;
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
                    ((IDisposable)this.UdpClient).Dispose();
                }

                this.UdpClient = null;

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
