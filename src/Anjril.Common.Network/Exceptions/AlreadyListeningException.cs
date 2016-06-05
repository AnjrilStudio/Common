namespace Anjril.Common.Network.Exceptions
{
    using System;

    public class AlreadyListeningException : Exception
    {
        #region constructors

        /// <summary>
        /// Instantiates a new already listening exception.
        /// </summary>
        public AlreadyListeningException()
            : base("This socket is already listening.")
        { }

        #endregion
    }
}
