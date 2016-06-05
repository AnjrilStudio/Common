﻿namespace Anjril.Common.Network.Exceptions
{
    using System;

    public class ConnectionLostException : Exception
    {
        #region constructors

        /// <summary>
        /// Instantiates a new connection lost exception.
        /// </summary>
        public ConnectionLostException(Exception cause)
            : base("The server you were connected with is no longer available.", cause)
        { }

        #endregion
    }
}