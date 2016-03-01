namespace Anjril.Common.Network.UdpImpl.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    internal class UdpHelper
    {
        #region properties

        private UdpClient Sender { get; set; }

        #endregion

        #region constructors

        public UdpHelper(UdpClient udpClient)
        {
            this.Sender = udpClient;
        }

        #endregion

        #region methods

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        public void SendMessage(Message message, IRemoteConnection destination)
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
                var datagram = this.SerializeMessage(message.ToString());
                this.Sender.Send(datagram, datagram.Length, new IPEndPoint(IPAddress.Parse(destination.IPAddress), destination.Port));
#if WAN
            }
#endif
        }

        public byte[] SerializeMessage(string message)
        {
            // TODO : parameterize the default encoding
            var datagram = Encoding.ASCII.GetBytes(message);

            return datagram;
        }

        public string DeserializeDatagram(byte[] datagram)
        {
            // TODO : parameterize the default encoding
            var message = Encoding.ASCII.GetString(datagram);

            return message;
        }

        #endregion
    }
}
