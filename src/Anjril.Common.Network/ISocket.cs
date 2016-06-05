namespace Anjril.Common.Network
{
    using System;
    using System.Collections.Generic;

    public interface ISocket : IDisposable
    {
        #region properties

        /// <summary>
        /// The port on which the socket is listening
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the socket is listening
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets a list that indicates all the client connected to the server
        /// </summary>
        IList<IRemoteConnection> Clients { get; }

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="ListeningPort"/>
        /// </summary>
        /// <param name="onConnectionRequested">the delegate that is executed when a connection request arrives</param>
        /// <param name="onMessageReceived">the delegate that is executed when a message from a connected client arrives</param>
        /// <param name="onDisconnect">the delegate that is executed when a remote is disconnected</param>
        /// <exception cref="Exceptions.AlreadyListeningException">If the socket is already listening</exception>
        void StartListening(ConnectionHandler onConnectionRequested, MessageHandler onMessageReceived, DisconnectionHandler onDisconnect);

        /// <summary>
        /// Stops the receiver from listening.
        /// </summary>
        void StopListening();

        /// <summary>
        /// Broadcasts the specified message to all the connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        void Broadcast(string message);

        #endregion
    }
}