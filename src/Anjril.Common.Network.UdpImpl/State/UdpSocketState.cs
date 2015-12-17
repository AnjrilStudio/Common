using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.State
{
    internal class UdpSocketState
    {
        #region properties

        /// <summary>
        /// List of connection waiting for a response and the elapsed time since the first request.
        /// </summary>
        internal List<ConnectionRequest> PendingConnections { get; private set; }

        /// <summary>
        /// List of message waiting for an acquittal and the elapsed time since the last shipment.
        /// </summary>
        internal List<AcquittalRequest> PendingAcquittals { get; private set; }

        /// <summary>
        /// List of remote connections currently connected with the socket represented by this state.
        /// </summary>
        internal List<IRemoteConnection> RemoteConnections { get; private set; }

        #endregion

        #region constructors

        public UdpSocketState()
        {
            this.RemoteConnections = new List<IRemoteConnection>();

            this.PendingConnections = new List<ConnectionRequest>();
            this.PendingAcquittals = new List<AcquittalRequest>();
        }

        #endregion
    }
}
