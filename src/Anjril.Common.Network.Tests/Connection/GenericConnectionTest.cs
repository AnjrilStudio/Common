using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Anjril.Common.Network;
using Anjril.Common.Network.Exceptions;

namespace Anjril.Common.Network.Tests.Connection
{
    [TestClass]
    public abstract class GenericConnectionTest<TServer, TClient>
        where TServer : ISocket, new()
        where TClient : ISocketClient, new()
    {
        private ISocketClient Client { get; set; }
        private ISocket Server { get; set; }

        private string MessageReceived { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Client = new TClient();
            this.Server = new TServer();
        }

        [TestMethod]
        public void TestConnectionUnreachable()
        {
            try
            {
                this.Client.Connect("127.0.0.1", Server.Port, null, "HELLO");
                Assert.Fail();
            }
            catch (ConnectionFailedException e)
            {
                Assert.AreEqual(TypeConnectionFailed.SocketUnreachable, e.TypeErreur);
                Assert.IsFalse(this.Client.IsConnected);
                Assert.AreEqual(0, this.Server.Clients.Count);
            }
        }

        [TestMethod]
        public void TestConnectionOk()
        {
            Server.StartListening(OnConnectionRequested, null, null);

            try
            {
                var response = this.Client.Connect("127.0.0.1", Server.Port, null, "HELLO");

                Assert.IsTrue(this.Client.IsConnected);
                Assert.AreEqual(1, this.Server.Clients.Count);
            }
            catch (ConnectionFailedException e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestConnectionRefused()
        {
            Server.StartListening(OnConnectionRequested, null, null);

            try
            {
                this.Client.Connect("127.0.0.1", Server.Port, null, "FAIL");
                Assert.Fail();
            }
            catch (ConnectionFailedException e)
            {
                Assert.AreEqual(TypeConnectionFailed.ConnectionRefused, e.TypeErreur);
                Assert.IsFalse(this.Client.IsConnected);
                Assert.AreEqual(0, this.Server.Clients.Count);
            }
        }

        private bool OnConnectionRequested(IRemoteConnection sender, string request, out string response)
        {
            if (request == "FAIL")
            {
                response = "REFUSED";
                return false;
            }
            else
            {
                response = "GRANTED";
                return true;
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Server.Dispose();
            this.Client.Dispose();
        }

    }
}
