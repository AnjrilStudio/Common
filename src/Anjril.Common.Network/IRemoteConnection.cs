namespace Anjril.Common.Network
{
    public interface IRemoteConnection
    {
        #region properties

        /// <summary>
        /// The IP address of the remote connection
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// The port used by the remote connection
        /// </summary>
        int Port { get; }

        #endregion

        #region methods

        /// <summary>
        /// Send a message to the remote connection
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <exception cref="Exceptions.CannotSendException">If the implementation can't send a message</exception>
        void Send(string message);

        #endregion
    }
}
