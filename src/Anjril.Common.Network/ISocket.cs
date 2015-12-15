namespace Anjril.Common.Network
{
    public interface ISocket : IReceiver, ISender
    {
        #region events

        /// <summary>
        /// Fires when a connection request arrives
        /// </summary>
        event MessageHandler OnConnection;

        /// <summary>
        /// Fires when a connection request is successful
        /// </summary>
        event MessageHandler OnConnected;

        /// <summary>
        /// Fires when a connection request fails
        /// </summary>
        event MessageHandler OnConnectionFailed;

        #endregion

        #region methods

        /// <summary>
        /// Connect to the specified remote connection
        /// </summary>
        /// <param name="pair">the pair to connect with</param>
        void Connect(IRemoteConnection pair);

        #endregion
    }
}