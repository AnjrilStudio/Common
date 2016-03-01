namespace Anjril.Common.Network.UdpImpl.Internal
{
    internal interface ISender
    {
        #region methods

        /// <summary>
        /// Sends a message to the specified destination
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="destination">the message destination</param>
        void Send(Message message, IRemoteConnection destination);

        #endregion
    }
}
