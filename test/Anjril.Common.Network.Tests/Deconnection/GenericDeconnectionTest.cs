using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Anjril.Common.Network;
using Anjril.Common.Network.Exceptions;

namespace Anjril.Common.Network.Tests.Deconnection
{
    [TestClass]
    public abstract class GenericDeconnectionTest<TServer, TClient>
        where TServer : ISocket, new()
        where TClient : ISocketClient, new()
    {
        private ISocketClient Client { get; set; }
        private ISocket Server { get; set; }

        private string MessageReceived { get; set; }
        private bool OnDisconnectCalled { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Client = new TClient();
            this.Server = new TServer();

            this.Server.StartListening(null, null, OnDisconnect);
            this.Client.Connect("127.0.0.1", this.Server.Port, null, null);

            this.OnDisconnectCalled = false;
            this.MessageReceived = null;
        }

        [TestMethod]
        public void TestClientCleanDisconnection()
        {
            this.Client.Disconnect("BYE");

            Assert.IsFalse(this.Client.IsConnected);
            Assert.AreEqual(0, this.Server.Clients.Count);
            Assert.IsTrue(this.OnDisconnectCalled);
            Assert.AreEqual("BYE", this.MessageReceived);

            this.Client.Connect("127.0.0.1", this.Server.Port, null, null);
        }

        [TestMethod]
        public void TestServerCleanDisconnection()
        {
            this.Server.CloseConnection(this.Server.Clients[0], "BYE");

            this.Client.Send("Test");
            try
            {
                this.Client.Send("Test");
            }
            catch (ConnectionLostException)
            {
                Assert.IsFalse(this.Client.IsConnected);
                Assert.AreEqual(0, this.Server.Clients.Count);
            }

            this.Client.Connect("127.0.0.1", this.Server.Port, null, null);
        }

        [TestMethod]
        public void TestClientLostServerConnection()
        {
            this.Server.Dispose();

            this.Client.Send("Test");
            try
            {
                this.Client.Send("Test");
            }
            catch (ConnectionLostException)
            {
                Assert.IsFalse(this.Client.IsConnected);
            }
        }

        private void OnDisconnect(IRemoteConnection remote, string justification)
        {
            this.OnDisconnectCalled = true;
            this.MessageReceived = justification;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Server.Dispose();
            this.Client.Dispose();
        }

    }
}
