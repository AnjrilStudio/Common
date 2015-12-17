using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.Exceptions
{
    public class NotConnectedException : Exception
    {
        #region properties

        public IRemoteConnection RemoteConnection { get; set; }

        #endregion

        #region constructors

        public NotConnectedException(IRemoteConnection remoteConnection)
            : base("You are not connected to the remote connection. Please use the Connect methods before sending any message.")
        {
            this.RemoteConnection = remoteConnection;
        }

        #endregion
    }
}
