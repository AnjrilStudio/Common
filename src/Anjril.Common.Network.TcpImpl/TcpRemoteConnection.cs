using Anjril.Common.Network.TcpImpl.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Anjril.Common.Network.TcpImpl
{
    public class TcpRemoteConnection : IRemoteConnection
    {
        #region properties

        public string IPAddress { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Address.ToString(); } }
        public int Port { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Port; } }

        /// <summary>
        /// The tcp client used to send and receive message
        /// </summary>
        internal TcpClient TcpClient { get; set; }

        #endregion

        #region private properties

        /// <summary>
        /// The buffer used to store partial messages received
        /// </summary>
        private string Buffer { get; set; }

        /// <summary>
        /// The separator used to parse the buffer and extract the messages
        /// </summary>
        private string Separator { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        public TcpRemoteConnection(TcpClient client, string separator)
        {
            this.TcpClient = client;
            this.Buffer = String.Empty;
            this.Separator = separator;
        }

        #endregion

        #region methods

        public void Send(string message)
        {
            message += this.Separator;

            var datagram = this.SerializeMessage(message);
            this.TcpClient.Client.Send(datagram);
        }

        /// <summary>
        /// Looks into the socket for a complete message and returns it. null if the message is incomplete.
        /// </summary>
        /// <returns>The received message. Null if not complete</returns>
        internal string Receive()
        {
            // Retrieving latest bytes
            if (this.TcpClient.Available > 0)
            {
                byte[] bytes = new byte[this.TcpClient.Available];

                this.TcpClient.GetStream().Read(bytes, 0, this.TcpClient.Available);

                this.Buffer += this.DeserializeDatagram(bytes);
            }

            string message = null;

            // Extracting next message
            if (this.Buffer.Contains(this.Separator))
            {
                var splitedMessages = this.Buffer.Split(new string[] { this.Separator }, StringSplitOptions.None);

                message = splitedMessages[0];
                this.Buffer = String.Join(this.Separator, splitedMessages.Skip(1).ToArray());
            }

            return message;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Serializes a string into a byte array
        /// </summary>
        /// <param name="message">The string to serialize</param>
        /// <returns>The string serialized into a byte array</returns>
        private byte[] SerializeMessage(string message)
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
        private string DeserializeDatagram(byte[] datagram)
        {
            // TODO : parameterize the default encoding
            var message = Encoding.ASCII.GetString(datagram);

            return message;
        }

        #endregion
    }
}
