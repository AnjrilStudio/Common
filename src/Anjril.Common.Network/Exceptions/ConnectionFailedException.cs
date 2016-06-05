namespace Anjril.Common.Network.Exceptions
{
    using System;

    public class ConnectionFailedException : Exception
    {
        #region properties

        /// <summary>
        /// The cause of the connection failure
        /// </summary>
        public TypeConnectionFailed TypeErreur { get; private set; }

        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new connection failed exception, with a timeout cause
        /// </summary>
        public ConnectionFailedException()
            : this(TypeConnectionFailed.Timeout, "Your connection request timeout.")
        { }

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters
        /// </summary>
        /// <param name="message">the cause of the connection failure</param>
        /// <param name="cause">The cause of the connection failure</param>
        public ConnectionFailedException(TypeConnectionFailed cause, string message)
            : base(message)
        {
            this.TypeErreur = cause;
        }

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters
        /// </summary>
        /// <param name="innerException">The exception that cause the connection failure</param>
        /// <param name="cause">The cause of the connection failure</param>
        public ConnectionFailedException(TypeConnectionFailed cause, Exception innerException)
            : base(innerException.Message, innerException)
        {
            this.TypeErreur = cause;
        }

        #endregion
    }

    public enum TypeConnectionFailed
    {
        Timeout,
        SocketUnreachable,
        Other,
        InvalidResponse,
        ConnectionRefused
    }
}