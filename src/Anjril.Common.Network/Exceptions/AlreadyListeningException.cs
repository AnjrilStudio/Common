using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
    public class AlreadyListeningException : Exception
    {
        #region properties

        public IReceiver Receiver { get; set; }

        #endregion

        #region constructors

        public AlreadyListeningException(IReceiver receiver)
            : base("This receiver is already listening.")
        {
            this.Receiver = receiver;
        }

        #endregion
    }
}
