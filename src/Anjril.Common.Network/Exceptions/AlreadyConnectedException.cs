namespace Anjril.Common.Network.Exceptions
{
    using System;

    public class AlreadyConnectedException : Exception
    {
        #region constructors

        public AlreadyConnectedException() : base("This instance is already connected to a socket. Please dispose it before connecting to another socket.")
        { }

        #endregion
    }
}
