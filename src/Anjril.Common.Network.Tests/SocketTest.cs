using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Anjril.Common.Network;
using Anjril.Common.Network.UdpImpl;
using System.Collections.Generic;
using System.Threading;
using Anjril.Common.Network.Tests.Utils;

namespace Anjril.Common.Network.Tests
{
    [TestClass]
    public class SocketTest
    {
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        #endregion

        #region properties

        public ISocket Server { get; set; }
        public ReceiverTester ServerTester { get; set; }

        public ISocket Client { get; set; }
        public ReceiverTester ClientTester { get; set; }

        public int ServerPort { get; private set; }
        public int ClientPort { get; private set; }

        #endregion

        public SocketTest()
        {
            this.ServerPort = 15000;
            this.ClientPort = 16000;
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void TestInitialize()
        {
            this.Server = new UdpSocket(this.ServerPort, this.OnServerConnection, this.OnServerConnected, this.OnServerConnectionFailed, null);
            this.Client = new UdpSocket(this.ClientPort, this.OnClientConnection, this.OnClientConnected, this.OnClientConnectionFailed, null);

            this.ClientTester = new ReceiverTester(this.Client);
            this.ServerTester = new ReceiverTester(this.Server);
        }

        #region server delegates

        private void OnServerConnectionFailed(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        private void OnServerConnected(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        private void OnServerConnection(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region client delegates

        private void OnClientConnectionFailed(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        private void OnClientConnected(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        private void OnClientConnection(IRemoteConnection sender, string message)
        {
            throw new NotImplementedException();
        }

        #endregion

        [TestMethod]
        public void TestAcquittement()
        {
            // To test this functionnality, ensure that the selected build configuration is WAN
            // This will simulate the package lost and order of arrival
            this.Server.StartListening();

            var remoteConnection = new UdpRemoteConnection("127.0.0.1", this.ServerPort);

            int nbMessage = 20;
            var messageSended = new List<string>();

            for (int i = 1; i <= nbMessage; i++)
            {
                var message = "This the test " + i;

                this.Client.Send(message, remoteConnection);
                messageSended.Add(message);
            }

            // Wait to ensure that all the messages has been processed
            Thread.Sleep(300);

            Assert.AreEqual(nbMessage, this.ServerTester.MessagedReceived.Count, "There is a difference between the number of sended messages and the number of received messages");

            foreach(var message in this.ServerTester.MessagedReceived)
            {
                Assert.IsTrue(messageSended.Contains(message.Item2), String.Format("Un message reçu ({0}) n'existe pas dans la liste des messages envoyés", message));
            }
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void TestCleanup()
        {
            this.Server.Dispose();
            this.Client.Dispose();
        }
    }
}
