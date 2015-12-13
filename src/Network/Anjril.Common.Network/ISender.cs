using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network
{
    public interface ISender
    {
        #region methods

        void Send(string message, RemoteConnection destination);

        #endregion
    }
}
