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
using Anjril.Common.Network.TcpImpl.Properties;

namespace Anjril.Common.Network.TcpTests
{
    [TestClass]
    public class TcpSocketConnectTests
    {
        private const int SERVER_PORT = 15000;
        private const int CLIENT_PORT = 16000;
        private const string SEPARATOR = "<sep>";
        private const string LOCALHOST = "127.0.0.1";

        private TcpSocket Tested { get; set; }
        private TcpRemoteConnection Tester { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            Settings.Default.ServerPort = SERVER_PORT;
            Settings.Default.MessageBound = SEPARATOR;

            var testerEndPoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), CLIENT_PORT);

            this.Tested = new TcpSocket();
            this.Tester = new TcpRemoteConnection(new TcpClient(testerEndPoint));
        }

        private Message ReceiveMessage()
        {
            Message message = null;
            var chrono = Stopwatch.StartNew();

            while (message == null)
            {
                message = this.Tester.Receive();

                if (chrono.ElapsedMilliseconds > 5000)
                {
                    chrono.Stop();
                    Assert.Fail("A message was expected");
                }
            }

            chrono.Stop();
            return message;
        }

        [TestMethod]
        public void TestConnectDisconnectOk()
        {
            var connectionGranted = "Granted";
            var connectionRequest = new Message(Command.ConnectionRequest, "Connection");
            var disconnectionRequest = new Message(Command.Disconnection, "Good Bye");

            ConnectionHandler onConnectionRequested = (IRemoteConnection sender, string request, out string response) =>
            {
                Assert.AreEqual(CLIENT_PORT, sender.Port);
                Assert.AreEqual(LOCALHOST, sender.IPAddress);
                Assert.AreEqual(connectionRequest.InnerMessage, request);

                response = connectionGranted;

                return true;
            };

            DisconnectionHandler onDisconnect = (remote, justification) =>
            {
                Assert.AreEqual(CLIENT_PORT, remote.Port);
                Assert.AreEqual(LOCALHOST, remote.IPAddress);
                Assert.AreEqual(disconnectionRequest.InnerMessage, justification);
            };

            this.Tested.StartListening(onConnectionRequested, null, onDisconnect);

            this.Tester.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);

            this.Tester.Send(connectionRequest);

            var connectionResponse = this.ReceiveMessage();

            Assert.IsTrue(connectionResponse.IsValid);
            Assert.AreEqual(Command.ConnectionGranted, connectionResponse.Command);
            Assert.AreEqual(connectionGranted, connectionResponse.InnerMessage);

            Assert.AreEqual(1, this.Tested.Clients.Count);

            this.Tester.Send(disconnectionRequest);

            var disconnectionResponse = this.ReceiveMessage();

            Assert.IsTrue(disconnectionResponse.IsValid);
            Assert.AreEqual(Command.Disconnected, disconnectionResponse.Command);

            Thread.Sleep(100);

            Assert.AreEqual(0, this.Tested.Clients.Count);
        }

        [TestMethod]
        public void TestConnectTimeout()
        {
            ConnectionHandler onConnectionRequested = (IRemoteConnection sender, string request, out string response) =>
            {
                Assert.Fail();

                response = "Granted";

                return true;
            };

            this.Tested.StartListening(onConnectionRequested, null, null);

            this.Tester.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);

            Thread.Sleep(2000);

            Assert.AreEqual(0, this.Tested.Clients.Count);

            var connectionResponse = this.ReceiveMessage();

            Assert.IsTrue(connectionResponse.IsValid);
            Assert.AreEqual(Command.ConnectionFailed, connectionResponse.Command);
        }

        [TestMethod]
        public void TestConnectRefused()
        {
            var connectionRefused = "Refused";
            var connectionRequest = new Message(Command.ConnectionRequest, "Connection");

            ConnectionHandler onConnectionRequested = (IRemoteConnection sender, string request, out string response) =>
            {
                Assert.AreEqual(CLIENT_PORT, sender.Port);
                Assert.AreEqual(LOCALHOST, sender.IPAddress);
                Assert.AreEqual(connectionRequest.InnerMessage, request);

                response = connectionRefused;

                return false;
            };

            this.Tested.StartListening(onConnectionRequested, null, null);

            this.Tester.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);

            this.Tester.Send(connectionRequest);

            Thread.Sleep(500);

            Assert.AreEqual(0, this.Tested.Clients.Count);

            var connectionResponse = this.ReceiveMessage();

            Assert.IsTrue(connectionResponse.IsValid);
            Assert.AreEqual(Command.ConnectionFailed, connectionResponse.Command);
            Assert.AreEqual(connectionRefused, connectionResponse.InnerMessage);
        }

        [TestMethod]
        public void TestConnectFailed1()
        {
            var connectionRequest = new Message(Command.Message, "Connection");

            ConnectionHandler onConnectionRequested = (IRemoteConnection sender, string request, out string response) =>
            {
                Assert.Fail();

                response = "Refused";

                return false;
            };

            this.Tested.StartListening(onConnectionRequested, null, null);

            this.Tester.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);

            this.Tester.Send(connectionRequest);

            Thread.Sleep(500);

            Assert.AreEqual(0, this.Tested.Clients.Count);

            var connectionResponse = this.ReceiveMessage();

            Assert.IsTrue(connectionResponse.IsValid);
            Assert.AreEqual(Command.ConnectionFailed, connectionResponse.Command);
        }

        [TestMethod]
        public void TestConnectFailed2()
        {
            ConnectionHandler onConnectionRequested = (IRemoteConnection sender, string request, out string response) =>
            {
                Assert.Fail();

                response = "Refused";

                return false;
            };

            this.Tested.StartListening(onConnectionRequested, null, null);

            this.Tester.TcpClient.Connect(IPAddress.Parse(LOCALHOST), SERVER_PORT);

            this.Tester.Send("This is a failed connection.");

            Thread.Sleep(500);

            Assert.AreEqual(0, this.Tested.Clients.Count);

            var connectionResponse = this.ReceiveMessage();

            Assert.IsTrue(connectionResponse.IsValid);
            Assert.AreEqual(Command.ConnectionFailed, connectionResponse.Command);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tested.Dispose();
            this.Tester.TcpClient.Dispose();
        }
    }
}
