using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class AlreadyListeningException : Exception
    {
        #region properties

        public ISocket Socket{ get; set; }

        #endregion

        #region constructors

        public AlreadyListeningException(ISocket socket)
            : base("This socket is already listening.")
        {
            this.Socket = socket;
        }

        #endregion
    }
}
