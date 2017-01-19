using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Anjril.Common.Network.TcpImpl;

namespace Anjril.Common.Network.Tests.Deconnection
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    [TestClass]
    public class TcpDeconnectionTest : GenericDeconnectionTest<TcpSocket, TcpSocketClient>
    { }
}
