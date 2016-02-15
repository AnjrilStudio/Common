namespace Anjril.Common.Network.UdpImpl
{
    using System;

    internal interface IReceiver : IDisposable
    {
        #region properties

        /// <summary>
        /// The port on which the receiver is listening
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the receiver is already listening
        /// </summary>
        bool IsListening { get; }

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

        /// <summary>
        /// Stops the receiver from listening.
        /// </summary>
        /// <remarks>A stopped receiver won't be able to listen again. You will have to create a new instance to listen again.</remarks>
        void StopListening();

        #endregion
    }
}
