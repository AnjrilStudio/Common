namespace Anjril.Common.Network
{
    using System;

    public interface ISocketClient : IDisposable
    {
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
        /// Connects to the specified remote server
        /// </summary>
        /// <param name="ipAddress">the IP address to connect with</param>
        /// <param name="port">the port to connect on</param>
        /// <param name="onMessageReceived">the delegate that is executed when a message is sended by the server to this client</param>
        /// <param name="message">an additionnal message to the request</param>
        /// <exception cref="Exceptions.ConnectionFailedException">When the SocketClient can't connect to the remote connection.</exception>
        /// <returns>the response of the server</returns>
        string Connect(string ipAddress, int port, MessageHandler onMessageReceived, string message);

        /// <summary>
        /// Shutdowns the connection with the remote server
        /// </summary>
        /// <param name="justification">the justification to send to the server</param>
        void Disconnect(string justification);

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        /// <exception cref="Exceptions.NotConnectedException">When the SocketClient is not connected</exception>
        /// <exception cref="Exceptions.ConnectionLostException">When the Server is no longer available</exception>
        void Send(string message);

        #endregion
    }
}
