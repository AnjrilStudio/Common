using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class AlreadyConnectedException : Exception
    {
        #region properties

        public ISocketClient Client{ get; set; }

        #endregion

        #region constructors

        public AlreadyConnectedException(ISocketClient client)
            : base("This client is already connected to a socket.")
        {
            this.Client = client;
        }

        #endregion
    }
}
