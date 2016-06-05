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
using System.Collections.Generic;

namespace Anjril.Common.Network.TcpTests.Client
{
    [TestClass]
    public class TcpSocketClientTests
    {
        private const int SERVER_PORT = 15000;
        private const int CLIENT_PORT = 16000;
        private const string SEPARATOR = "<sep>";
        private const string LOCALHOST = "127.0.0.1";

        private TcpSocketClient Tested { get; set; }
        private TcpListener TesterListener { get; set; }
        private TcpRemoteConnection Tester { get; set; }

        private IList<Tuple<string, IRemoteConnection>> ReceivedMessages { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.ReceivedMessages = new List<Tuple<string, IRemoteConnection>>();

            this.TesterListener = new TcpListener(new IPEndPoint(IPAddress.Parse(LOCALHOST), SERVER_PORT));
            this.Tested = new TcpSocketClient(CLIENT_PORT, SEPARATOR);

            this.TesterListener.Start();

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();
            var connectionResponse = Task.Run(() => this.Tested.Connect(LOCALHOST, SERVER_PORT, OnMessageReceived, null));

            // wait until the tester receive the connection request
            connectionRequest.Wait(2000);

            if (!connectionRequest.IsCompleted)
            {
                connectionResponse.Wait();
            }

            this.Tester = new TcpRemoteConnection(connectionRequest.Result, SEPARATOR);

            var messageRequest = this.ReceiveMessage();

            this.Tester.Send(new Message(Command.ConnectionGranted, null).ToString());

            // wait until the tested receive the connection response
            connectionResponse.Wait();
        }

        /// <summary>
        /// Receives a message from the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnMessageReceived(IRemoteConnection sender, string message)
        {
            this.ReceivedMessages.Add(Tuple.Create(message, sender));
        }

        /// <summary>
        /// Receives a message from the tester
        /// </summary>
        /// <returns></returns>
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
        public void TestSend()
        {
            var message = "Test";

            for (int i = 0; i < 10; i++)
            {
                this.Tested.Send(message + i);
            }

            for (int i = 0; i < 10; i++)
            {
                var response = this.ReceiveMessage();

                Assert.AreEqual(message + i, response.InnerMessage);
            }
        }

        [TestMethod]
        public void TestDisconnectedServer()
        {
            this.Tester.TcpClient.Dispose();

            this.Tested.Send("Test");
            try
            {
                this.Tested.Send("Test");
                Assert.Fail();
            }
            catch (ConnectionLostException e)
            {
                Assert.IsTrue(e.InnerException is SocketException);
                Assert.AreEqual(10053, (e.InnerException as SocketException).ErrorCode);
            }

            Assert.IsFalse(this.Tested.IsConnected);
        }

        [TestMethod]
        public void TestReceive()
        {
            var message = "Test";

            for (int i = 0; i < 10; i++)
            {
                this.Tester.Send(new Message(Command.Message, message + i).ToString());
            }

            // Wait until all the messages have been received
            Thread.Sleep(200);

            for (int i = 0; i < 10; i++)
            {
                var pair = this.ReceivedMessages[i];

                Assert.AreEqual(message + i, pair.Item1);
                Assert.AreEqual(SERVER_PORT, pair.Item2.Port);
                Assert.AreEqual(LOCALHOST, pair.Item2.IPAddress);
            }
        }

        [TestMethod]
        public void TestCleanDisconnect()
        {
            var message = "Disconnection";
            var disconnection = Task.Run(() => this.Tested.Disconnect(message));

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);
            Assert.AreEqual(message, disconnectionRequest.InnerMessage);

            var disconnectionReponse = new Message(Command.Disconnected, null);
            this.Tester.Send(disconnectionReponse.ToString());

            this.Tester.TcpClient.Dispose();

            disconnection.Wait();

            Assert.IsFalse(this.Tested.IsConnected);
        }

        [TestMethod]
        public void TestDisconnectServerFail()
        {
            var message = "Disconnection";
            var disconnection = Task.Run(() => this.Tested.Disconnect(message));

            var disconnectionRequest = this.ReceiveMessage();

            Assert.IsTrue(disconnectionRequest.IsValid);
            Assert.AreEqual(Command.Disconnection, disconnectionRequest.Command);
            Assert.AreEqual(message, disconnectionRequest.InnerMessage);

            this.Tester.TcpClient.Dispose();

            disconnection.Wait();

            Assert.IsFalse(this.Tested.IsConnected);
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
