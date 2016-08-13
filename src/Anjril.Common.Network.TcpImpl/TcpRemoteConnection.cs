namespace Anjril.Common.Network.TcpImpl
{
    using Exceptions;
    using global::Common.Logging;
    using Internals;
    using Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;


    public class TcpRemoteConnection : IRemoteConnection
    {
        private static ILog log = LogManager.GetLogger(typeof(TcpRemoteConnection));

        #region properties

        public string IPAddress { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Address.ToString(); } }
        public int Port { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Port; } }
        public long Ping
        {
            get
            {
                return this.PingUtil.Send((this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Address).RoundtripTime;
            }
        }

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
        /// The ping instance used to get the ping between two remote connection
        /// </summary>
        private Ping PingUtil { get; set; }

        /// <summary>
        /// The TcpSocket which is managin this instance of TcpRemoteConnection
        /// </summary>
        private TcpSocket TcpSocket { get; set; }

        /// <summary>
        /// The TcpSocketClient which is managing this instance of TcpRemoteConnection
        /// </summary>
        private TcpSocketClient TcpSocketClient { get; set; }

        /// <summary>
        /// The Encoding used to serialize messages
        /// </summary>
        private Encoding Encoder { get; set; }

        #endregion

        #region constructors

        /// <summary>
        ///Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        internal TcpRemoteConnection(TcpClient client)
        {
            this.TcpClient = client;
            this.Buffer = String.Empty;

            this.Encoder = Encoding.GetEncoding(Settings.Default.Encoding);

            this.PingUtil = new Ping();
        }

        /// <summary>
        /// Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        /// <param name="socket">The socket managing the new instance</param>
        public TcpRemoteConnection(TcpClient client, TcpSocket socket)
            : this(client)
        {
            this.TcpSocket = socket;
        }

        /// <summary>
        /// Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        /// <param name="socketClient">The socketClient managing the new instance</param>
        public TcpRemoteConnection(TcpClient client, TcpSocketClient socketClient)
            : this(client)
        {
            this.TcpSocketClient = socketClient;
        }

        #endregion

        #region methods

        public void Send(string message)
        {
            var msg = new Message(Command.Message, message);
            this.Send(msg);
        }

        /// <summary>
        /// Looks into the socket for a complete message and returns it. null if the message is incomplete.
        /// </summary>
        /// <returns>The received message. Null if not complete</returns>
        internal Message Receive()
        {
            // Retrieving latest bytes
            if (this.TcpClient.Available > 0)
            {
                byte[] bytes = new byte[this.TcpClient.Available];

                this.TcpClient.GetStream().Read(bytes, 0, bytes.Length);

                string addition = this.DeserializeDatagram(bytes);

                log.TraceFormat("Addition of '{0}' to the buffer.", addition);

                this.Buffer += addition;
            }

            Message message = null;

            var separator = Settings.Default.MessageBound;

            // Extracting next message
            if (this.Buffer.Contains(separator))
            {
                var splitedMessages = this.Buffer.Split(new string[] { separator }, StringSplitOptions.None);

                log.TraceFormat("Delivery of a new message : '{0}'", splitedMessages[0]);

                message = new Message(splitedMessages[0]);

                this.Buffer = String.Join(separator, splitedMessages.Skip(1).ToArray());
            }

            return message;
        }

        /// <summary>
        /// Sends the specified message to the remote connection
        /// </summary>
        /// <param name="message"></param>
        internal void Send(Message message)
        {
            log.DebugFormat("New message ({0}) sent to the remote connection: {1}:{2}", message, this.IPAddress, this.Port);

            var datagram = this.SerializeMessage(message.ToString() + Settings.Default.MessageBound);

            try
            {
                this.TcpClient.Client.Send(datagram);
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case 10053:
                        // The remote connection is disconnected
                        if (this.TcpSocket != null)
                        {
                            this.TcpSocket.Clients.Remove(this);
                            this.TcpSocket.OnDisconnect?.Invoke(this, null);
                        }
                        else if (this.TcpSocketClient != null)
                        {
                            this.TcpSocketClient.ResetConnection();
                            throw new ConnectionLostException(e);
                        }
                        break;
                    default:
                        // In all the other cases, the exception is thrown
                        throw e;
                }
            }
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
            var datagram = this.Encoder.GetBytes(message);

            return datagram;
        }

        /// <summary>
        /// Deserializes a byte array into a string
        /// </summary>
        /// <param name="datagram">The byte array to deserialize</param>
        /// <returns>The string deserialized</returns>
        private string DeserializeDatagram(byte[] datagram)
        {
            var message = this.Encoder.GetString(datagram);

            return message;
        }

        #endregion

        #region overridden methods

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IRemoteConnection))
                return false;

            IRemoteConnection other = (IRemoteConnection)obj;

            return other.IPAddress == this.IPAddress && other.Port == this.Port;
        }

        public override int GetHashCode()
        {
            return this.TcpClient.GetHashCode();
        }

        #endregion
    }
}
