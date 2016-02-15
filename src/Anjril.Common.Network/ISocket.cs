namespace Anjril.Common.Network
{
    using System;

    public interface ISocket : IDisposable
    {
        #region events

        /// <summary>
        /// Fires when a connection request arrives
        /// </summary>
        event MessageHandler OnConnection;

        /// <summary>
        /// Fires when a message arrives
        /// </summary>
        event MessageHandler OnReceive;

        #endregion

        #region properties

        /// <summary>
        /// The port on which the socket is listening
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the socket is already listening
        /// </summary>
        bool IsListening { get; }

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="ListeningPort"/>
        /// </summary>
        /// <exception cref="Exceptions.AlreadyListeningException">If the socket is already listening</exception>
        void StartListening();

        /// <summary>
        /// Stops the receiver from listening.
        /// </summary>
        /// <remarks>A stopped receiver won't be able to listen again. You will have to create a new instance to listen again.</remarks>
        void StopListening();

        /// <summary>
        /// Broadcasts the specified message to all the connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        void Broadcast(string message);

        #endregion
    }
}