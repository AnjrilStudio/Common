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
    public class TcpSocketClientConnectTests
    {
        private const int SERVER_PORT = 15000;
        private const int CLIENT_PORT = 16000;
        private const string SEPARATOR = "<sep>";
        private const string LOCALHOST = "127.0.0.1";

        private TcpSocketClient Tested { get; set; }
        private TcpListener TesterListener { get; set; }
        private TcpRemoteConnection Tester { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.TesterListener = new TcpListener(new IPEndPoint(IPAddress.Parse(LOCALHOST), SERVER_PORT));
            this.Tested = new TcpSocketClient(CLIENT_PORT, SEPARATOR);

            this.TesterListener.Start();
        }

        private Message ReceiveMessage()
        {
            string message = null;
            var chrono = Stopwatch.StartNew();

            while (String.IsNullOrWhiteSpace(message))
            {
                message = this.Tester.Receive();

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
        public void TestConnectOk()
        {
            var request = "Connection";
            var response = "Granted";

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, null, request));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            Assert.IsTrue(messageRequest.IsValid);
            Assert.AreEqual(Command.ConnectionRequest, messageRequest.Command);
            Assert.AreEqual(request, messageRequest.InnerMessage);

            this.Tester.Send(new Message(Command.ConnectionGranted, response).ToString());

            // wait until the tested receive the connection response
            connectionResponse.Wait();

            Assert.AreEqual(connectionResponse.Result, response);
            Assert.IsTrue(this.Tested.IsConnected);
        }

        [TestMethod]
        public void TestConnectRefused()
        {
            var request = "Connection";
            var response = "Refused";

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, null, request));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            Assert.IsTrue(messageRequest.IsValid);
            Assert.AreEqual(Command.ConnectionRequest, messageRequest.Command);
            Assert.AreEqual(request, messageRequest.InnerMessage);

            this.Tester.Send(new Message(Command.ConnectionFailed, response).ToString());

            Thread.Sleep(200);

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsNotNull(disconnectionRequest);
            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);

            this.Tester.Send(new Message(Command.Disconnected, null).ToString());

            this.Tester.TcpClient.Dispose();

            try
            {
                // wait until the tested receive the connection response
                connectionResponse.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var e = (ConnectionFailedException)aggregateException.InnerExceptions[0];

                Assert.AreEqual(TypeConnectionFailed.ConnectionRefused, e.TypeErreur);
                Assert.IsFalse(this.Tested.IsConnected);
                Assert.AreEqual(response, e.Message);
            }
        }

        [TestMethod]
        public void TestConnectTimeout()
        {
            var request = "Connection";

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, null, request));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            Assert.IsTrue(messageRequest.IsValid);
            Assert.AreEqual(Command.ConnectionRequest, messageRequest.Command);
            Assert.AreEqual(request, messageRequest.InnerMessage);

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsNotNull(disconnectionRequest);
            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);

            this.Tester.Send(new Message(Command.Disconnected, null).ToString());

            this.Tester.TcpClient.Dispose();

            try
            {
                // wait until the tested receive the connection response
                connectionResponse.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var e = (ConnectionFailedException)aggregateException.InnerExceptions[0];

                Assert.AreEqual(TypeConnectionFailed.Timeout, e.TypeErreur);
                Assert.IsFalse(this.Tested.IsConnected);
            }
        }

        [TestMethod]
        public void TestConnectFailed1()
        {
            var request = "Connection";

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, null, request));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            Assert.IsTrue(messageRequest.IsValid);
            Assert.AreEqual(Command.ConnectionRequest, messageRequest.Command);
            Assert.AreEqual(request, messageRequest.InnerMessage);

            this.Tester.Send(new Message(Command.Message, null).ToString());

            Thread.Sleep(200);

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsNotNull(disconnectionRequest);
            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);

            this.Tester.Send(new Message(Command.Disconnected, null).ToString());

            this.Tester.TcpClient.Dispose();

            try
            {
                // wait until the tested receive the connection response
                connectionResponse.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var e = (ConnectionFailedException)aggregateException.InnerExceptions[0];

                Assert.AreEqual(TypeConnectionFailed.Other, e.TypeErreur);
                Assert.IsFalse(this.Tested.IsConnected);
            }
        }

        [TestMethod]
        public void TestConnectFailed2()
        {
            var request = "Connection";

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, null, request));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            Assert.IsTrue(messageRequest.IsValid);
            Assert.AreEqual(Command.ConnectionRequest, messageRequest.Command);
            Assert.AreEqual(request, messageRequest.InnerMessage);

            this.Tester.Send("This is an unvalid reponse");

            Thread.Sleep(200);

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsNotNull(disconnectionRequest);
            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);

            this.Tester.Send(new Message(Command.Disconnected, null).ToString());

            this.Tester.TcpClient.Dispose();

            try
            {
                // wait until the tested receive the connection response
                connectionResponse.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var e = (ConnectionFailedException)aggregateException.InnerExceptions[0];

                Assert.AreEqual(TypeConnectionFailed.InvalidResponse, e.TypeErreur);
                Assert.IsFalse(this.Tested.IsConnected);
            }
        }

        [TestMethod]
        public void TestConnectUnreachable()
        {
            var request = "Connection";

            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, 60000, null, request));

            try
            {
                // wait until the tested receive the connection response
                connectionResponse.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var e = (ConnectionFailedException)aggregateException.InnerExceptions[0];

                Assert.AreEqual(TypeConnectionFailed.SocketUnreachable, e.TypeErreur);
                Assert.IsFalse(this.Tested.IsConnected);
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.TesterListener.Stop();
            this.TesterListener.Server.Dispose();
            if (this.Tester != null)
                this.Tester.TcpClient.Dispose();
            this.Tested.Dispose();
        }
    }
}
