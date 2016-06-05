using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.UdpImpl.Internal
{
    internal interface ISocketHelper : IDisposable
    {
        #region properties

        /// <summary>
        /// The port used by the socket to send and receive messages
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the socket is already listening
        /// </summary>
        bool IsListening { get; }

        #endregion

        #region events

        /// <summary>
        /// Fires when a message arrives
        /// </summary>
        event InternalMessageHandler OnReceive;

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="Port"/> on a second thread. Subscribe to the <see cref="OnReceive"/> event to consults them.
        /// </summary>
        /// <exception cref="Exceptions.SocketHelperAlreadyListeningException"></exception>
        void StartListening();

        /// <summary>
        /// Stops the socket from listening.
        /// </summary>
        /// <remarks>A stopped socket won't be able to listen again. You will have to create a new instance to listen again.</remarks>
        void StopListening();

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        void Send(Message message, UdpRemoteConnection destination);

        #endregion
    }
}
