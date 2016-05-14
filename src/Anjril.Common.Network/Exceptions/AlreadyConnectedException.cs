using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class AlreadyConnectedException : Exception
    {
        #region constructors

        public AlreadyConnectedException() : base("This instance is already connected to a socket. Please dispose it before connecting to another socket.")
        { }

        #endregion
    }
}
