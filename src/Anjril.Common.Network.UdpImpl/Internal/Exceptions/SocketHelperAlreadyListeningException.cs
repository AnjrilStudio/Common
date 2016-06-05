namespace Anjril.Common.Network.UdpImpl.Internal.Exceptions
{
    using System;
    internal class SocketHelperAlreadyListeningException : Exception
    {
        #region properties

        public ISocketHelper SocketHelper { get; set; }

        #endregion

        #region constructors

        public SocketHelperAlreadyListeningException(ISocketHelper socketHelper)
            : base("This socket helper is already listening.")
        {
            this.SocketHelper = socketHelper;
        }

        #endregion
    }
}