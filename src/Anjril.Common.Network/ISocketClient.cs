namespace Anjril.Common.Network
{
    using System;

    public interface ISocketClient : IDisposable
    {
        #region events

        /// <summary>
        /// Fires when a message arrives
        /// </summary>
        event MessageHandler OnReceive;

        /// <summary>
        /// Fires when the connection request is successful
        /// </summary>
        event MessageHandler OnConnected;

        #endregion

        #region properties

        /// <summary>
        /// The port from which the client is sending messages and listening answers
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the socket is already connected
        /// </summary>
        bool IsConnected { get; }

        #endregion

        #region methods 

        /// <summary>
        /// Connects to the specified remote connection
        /// </summary>
        /// <param name="pair">the pair to connect with</param>
        /// <exception cref="Exceptions.AlreadyConnectedException">If the client is already connected to a socket</exception>
        void Connect(IRemoteConnection pair);

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        void Send(string message);

        #endregion
    }
}
