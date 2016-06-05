namespace Anjril.Common.Network.UdpImpl.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    internal class UdpRemoteConnection : IRemoteConnection
    {
        #region properties

        public string IPAddress { get { return this.EndPoint.Address.ToString(); } }
        public int Port { get { return this.EndPoint.Port; } }
        
        #endregion

        #region private properties

        /// <summary>
        /// The underlying endpoint used by this UDP implementation
        /// </summary>
        internal IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// The SocketHelper used to send messages to this remote connection
        /// </summary>
        internal ISocketHelper SocketHelper { get; set; }

        /// <summary>
        /// The id of the next message to send
        /// </summary>
        internal ulong NextSendingId { get; set; }

        /// <summary>
        /// The expected id of the next message
        /// </summary>
        internal ulong NextReceivingId { get; set; }

        /// <summary>
        /// The list of arrived message, waiting for former message not arrived yet
        /// </summary>
        private IList<Message> MessageStack { get; set; }

        #endregion

        #region constructors

        public UdpRemoteConnection(IPEndPoint endPoint, ISocketHelper socketHelper)
        {
            this.EndPoint = endPoint;
            this.SocketHelper = socketHelper;

            this.NextReceivingId = this.NextSendingId = UInt64.MinValue;
            this.MessageStack = new List<Message>();
        }

        #endregion

        #region methods

        public void Send(string message)
        {
            Message msg = new Message(this.NextSendingId, Command.Other, message);
            this.IncrementSendingId();

            this.Send(msg);
        }

        internal void Send(Message message)
        {
            this.SocketHelper.Send(message, this);
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
            return this.EndPoint.GetHashCode();
        }

        #endregion
    }
}
