using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.Exceptions
{
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
