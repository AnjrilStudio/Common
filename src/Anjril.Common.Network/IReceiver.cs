using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network
{
    public interface IReceiver
    {
        #region properties

        int ListeningPort { get; }

        #endregion

        #region events

        event ReceiveHandler OnReceive;

        #endregion

        #region methods

        void StartListening();

        #endregion
    }
}
