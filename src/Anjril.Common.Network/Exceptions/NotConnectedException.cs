using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class NotConnectedException : Exception
    {
        #region constructors

        public NotConnectedException()
            : base("You are not connected to any remote connection. Please use the Connect methods before sending any message.")
        { }

        #endregion
    }
}
