namespace Anjril.Common.Network.UdpImpl.Internal
{
    using Anjril.Common.Network.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Linq;

    public class UdpRemoteConnection : IRemoteConnection
    {
        #region properties

        public string IPAddress { get { return this.EndPoint.Address.ToString(); } }
        public int Port { get { return this.EndPoint.Port; } }

        internal IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// The sender used to send messages to this remote connection
        /// </summary>
        internal ISender Sender { get; set; }

        /// <summary>
        /// The id of the next message to send
        /// </summary>
        internal ulong NextSendingId { get; private set; }

        /// <summary>
        /// The expected id of the next message
        /// </summary>
        internal ulong NextReceivingId { get; private set; }

        /// <summary>
        /// The list of arrived message, waiting for former message not arrived yet
        /// </summary>
        internal IList<Message> MessageStack { get; set; }

        #endregion

        #region constructors

        public UdpRemoteConnection(string ipAddress, int port)
            : this(new IPEndPoint(UdpRemoteConnection.Parse(ipAddress), port))
        { }

        public UdpRemoteConnection(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;

            this.NextReceivingId = this.NextSendingId = UInt64.MinValue;
            this.MessageStack = new List<Message>();
        }

        #endregion

        #region methods

        public void Send(string message)
        {
            // TODO : calculate next id
            Message msg = new Message(0, Command.Other, message);

            this.Send(msg);
        }

        internal void Send(Message message)
        {
            this.Sender.Send(message, this);
        }

        private static IPAddress Parse(string ipAddress)
        {
            var addressPart = ipAddress.Split('.');

            return new IPAddress(addressPart.Select(part => byte.Parse(part)).ToArray());
        }

        internal void IncrementReceivingId()
        {
            if (this.NextReceivingId == UInt64.MaxValue)
            {
                this.NextReceivingId = UInt64.MinValue;
            }
            else
            {
                this.NextReceivingId++;
            }
        }

        internal void IncrementSendingId()
        {
            if (this.NextSendingId == UInt64.MaxValue)
            {
                this.NextSendingId = UInt64.MinValue;
            }
            else
            {
                this.NextSendingId++;
            }
        }

        #endregion

        #region overrided methods

        public override bool Equals(object obj)
        {
            if(!(obj is IRemoteConnection))
            {
                return false;
            }

            IRemoteConnection target = (IRemoteConnection)obj;

            return target.IPAddress == this.IPAddress && target.Port == this.Port;
        }

        public override int GetHashCode()
        {
            return this.EndPoint.GetHashCode();
        }

        #endregion
    }
}
