using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network
{
    public interface IReceiver
    {
        #region properties

        /// <summary>
        /// The port on which the receiver is listening
        /// </summary>
        int ListeningPort { get; }

        #endregion

        #region events

        /// <summary>
        /// Fires when a message arrives
        /// </summary>
        event MessageHandler OnReceive;

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="ListeningPort"/>
        /// </summary>
        void StartListening();

        #endregion
    }
}
