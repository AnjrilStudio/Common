using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class CannotSendException : Exception
    {
        #region properties

        public IRemoteConnection RemoteConnection { get; set; }

        #endregion

        #region constructors

        public CannotSendException(IRemoteConnection remoteConnection)
            : base("This remote connection can't send a message. Your Receiver implementation is not compatible with this functionality.")
        {
            this.RemoteConnection = remoteConnection;
        }

        #endregion
    }
}
