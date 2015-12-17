﻿using Anjril.Common.Network.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpRemoteConnection : IRemoteConnection
    {
        #region properties

        public string IPAddress { get; private set; }
        public int Port { get; private set; }
        public bool CanSend { get { return this.Sender != null; } }

        /// <summary>
        /// The sender used to send messages to this remote connection
        /// </summary>
        internal ISender Sender { get; set; }

        /// <summary>
        /// The id of the next message to send
        /// </summary>
        internal ulong NextSendingId { get; set; }

        /// <summary>
        /// The expected id of the next message
        /// </summary>
        internal ulong NextReceivingId { get; set; }

        #endregion

        #region constructors

        public UdpRemoteConnection(string ipAddress, int port)
        {
            this.IPAddress = ipAddress;
            this.Port = port;

            this.NextReceivingId = this.NextSendingId = UInt64.MinValue;
        }

        public UdpRemoteConnection(IPEndPoint endPoint)
            : this(endPoint.Address.ToString(), endPoint.Port)
        { }

        #endregion

        #region methods

        public void Send(string message)
        {
            if (!this.CanSend)
                throw new CannotSendException(this);

            this.Sender.Send(message, this);
        }

        #endregion
    }
}