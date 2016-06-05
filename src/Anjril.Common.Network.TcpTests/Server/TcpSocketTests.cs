using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using Anjril.Common.Network.TcpImpl;
using System.Threading;
using System.Text;
using Anjril.Common.Network.Exceptions;
using System.Threading.Tasks;
using Anjril.Common.Network.TcpImpl.Internals;
using System.Diagnostics;

namespace Anjril.Common.Network.TcpTests
{
    [TestClass]
    public class TcpSocketTests
    {
        private const int SERVER_PORT = 15000;
        private const int CLIENT_1_PORT = 16000;
        private const int CLIENT_2_PORT = 17000;
        private const string SEPARATOR = "<sep>";
        private const string LOCALHOST = "127.0.0.1";

        private TcpSocket Tested { get; set; }
        private TcpRemoteConnection Tester1 { get; set; }
        private TcpRemoteConnection Tester2 { get; set; }

        private TcpRemoteConnection CurrentSender { get; set; }
        private string CurrentMessage { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            var tester1EndPoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), CLIENT_1_PORT);
            var tester2EndPoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), CLIENT_2_PORT);

            this.Tested = new TcpSocket(SERVER_PORT, SEPARATOR);
            this.Tester1 = new TcpRemoteConnection(new TcpClient(tester1EndPoint), SEPARATOR);
            this.Tester2 = new TcpRemoteConnection(new TcpClient(tester2EndPoint), SEPARATOR);

            this.Tested.StartListening(null, OnMessageReceived, null);

            var connectionRequest = new Message(Command.ConnectionRequest, "Connection");

            this.Tester1.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);
            this.Tester1.Send(connectionRequest.ToString());
            this.ReceiveMessage(this.Tester1);

            this.Tester2.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);
            this.Tester2.Send(connectionRequest.ToString());
            this.ReceiveMessage(this.Tester2);
        }

        private void OnMessageReceived(IRemoteConnection sender, string message)
        {
            Assert.AreEqual(this.CurrentMessage, message);
            Assert.AreEqual(this.CurrentSender.Port, sender.Port);
            Assert.AreEqual(this.CurrentSender.IPAddress, sender.IPAddress);
        }

        private Message ReceiveMessage(TcpRemoteConnection tester)
        {
            string message = null;
            var chrono = Stopwatch.StartNew();

            while (String.IsNullOrWhiteSpace(message))
            {
                message = tester.Receive();

                if (chrono.ElapsedMilliseconds > 5000)
                {
                    chrono.Stop();
                    Assert.Fail("A message was expected");
                }
            }

            chrono.Stop();
            return new Message(message);
        }

        [TestMethod]
        public void TestReceive()
        {
            this.CurrentSender = this.Tester1;
            this.CurrentMessage = "Message 1.1";

            this.Tester1.Send(this.CurrentMessage);

            this.CurrentMessage = "Message 1.2";

            this.Tester1.Send(this.CurrentMessage);

            this.CurrentSender = this.Tester2;
            this.CurrentMessage = "Message 2.1";

            this.Tester2.Send(this.CurrentMessage);

            this.CurrentSender = this.Tester1;
            this.CurrentMessage = "Message 1.3";

            this.Tester1.Send(this.CurrentMessage);

            this.CurrentSender = this.Tester2;
            this.CurrentMessage = "Message 2.2";

            this.Tester2.Send(this.CurrentMessage);
        }

        [TestMethod]
        public void TestDisconnectedClient()
        {
            this.Tester1.TcpClient.Dispose();

            this.Tested.Broadcast("Test");
            this.Tested.Broadcast("Test");

            Assert.AreEqual(1, this.Tested.Clients.Count);
        }

        [TestMethod]
        public void TestBroadcast()
        {
            for (int i = 0; i < 10; i++)
            {
                var message = "Broadcast " + i;
                this.Tested.Broadcast(message);

                var messageReceived = this.ReceiveMessage(this.Tester1);
                Assert.IsTrue(messageReceived.IsValid);
                Assert.AreEqual(Command.Message, messageReceived.Command);
                Assert.AreEqual(message, messageReceived.InnerMessage);

                messageReceived = this.ReceiveMessage(this.Tester2);
                Assert.IsTrue(messageReceived.IsValid);
                Assert.AreEqual(Command.Message, messageReceived.Command);
                Assert.AreEqual(message, messageReceived.InnerMessage);
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tested.Dispose();
            this.Tester1.TcpClient.Dispose();
            this.Tester2.TcpClient.Dispose();
        }
    }
}
