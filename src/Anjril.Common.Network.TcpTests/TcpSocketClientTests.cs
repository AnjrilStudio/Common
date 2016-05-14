using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using Anjril.Common.Network.TcpImpl;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Anjril.Common.Network.TcpImpl.Internals;
using Anjril.Common.Network.Exceptions;
using System.Collections.Generic;

namespace Anjril.Common.Network.TcpTests
{
    [TestClass]
    public class TcpSocketClientTests
    {
        private string Separator { get; set; }
        private Tuple<IRemoteConnection, string>[] Messages { get; set; }
        private TcpSocketClient Tested { get; set; }
        private TcpListener TesterListener { get; set; }
        private TcpRemoteConnection TesterTcpRemoteConnection { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Separator = "<sep>";

            this.Messages = new Tuple<IRemoteConnection, string>[10];

            this.Tested = new TcpSocketClient(16000, this.Separator);

            var testerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            this.TesterListener = new TcpListener(testerEndPoint);

            this.TesterListener.Start();
            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            // creation of the connection
            var connect = Task.Run(() =>
            {
                this.Tested.Connect("127.0.0.1", 15000, OnMessageReceived, "Test");
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

            this.TesterTcpRemoteConnection = new TcpRemoteConnection(connectionRequest.Result, this.Separator);

            var remoteTester = new TcpRemoteConnection(connectionRequest.Result, this.Separator);
            string message = null;

            // Wait until the effective connection request arrives to the tester
            while (String.IsNullOrWhiteSpace(message))
            {
                Thread.Sleep(50);
                message = remoteTester.Receive();
            }

            remoteTester.Send(new Message(Command.ConnectionGranted, "Ok").ToString());

            connect.Wait();
        }

        /// <summary>
        /// Methods called when a message is received by the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnMessageReceived(IRemoteConnection sender, string message)
        {
            for (int i = 0; i < 10; i++)
            {
                if (this.Messages[i] == null)
                {
                    this.Messages[i] = Tuple.Create(sender, message);
                    break;
                }
            }
        }

        [TestMethod]
        public void TestSend()
        {
            string message = "This is a test";

            for (int i = 0; i < 10; i++)
            {
                this.Tested.Send(message + i);
            }

            for (int i = 0; i < 10; i++)
            {
                Message messageReceived = null;
                while (messageReceived == null)
                {
                    string messageReceivedStr = this.TesterTcpRemoteConnection.Receive();

                    if (!String.IsNullOrWhiteSpace(messageReceivedStr))
                    {
                        messageReceived = new Message(messageReceivedStr);
                    }

                    Assert.IsTrue(messageReceived.IsValid);
                    Assert.AreEqual(Command.Message, messageReceived.Command);
                    Assert.AreEqual(message + i, messageReceived.InnerMessage);
                }
            }
        }

        [TestMethod]
        public void TestReceive()
        {
            string message = "This is a test";

            for (int i = 0; i < 10; i++)
            {
                var msg = new Message(Command.Message, message + i);
                this.TesterTcpRemoteConnection.Send(msg.ToString());
            }

            // Wait until all the messages are sended to the tested instance
            Thread.Sleep(500);

            for (int i = 0; i < 10; i++)
            {
                var tuple = this.Messages[i];

                Assert.IsNotNull(tuple);
                Assert.AreEqual(tuple.Item1.IPAddress, "127.0.0.1");
                Assert.AreEqual(tuple.Item1.Port, 15000);
                Assert.AreEqual(message + i, tuple.Item2);
            }
        }

        [TestMethod]
        public void TestDisconnect()
        {
            this.Tested.Disconnect("Good Bye");

            string disconnectionStr = null;

            // Wait until the effective disconnection message arrives to the tester
            while (String.IsNullOrWhiteSpace(disconnectionStr))
            {
                Thread.Sleep(50);
                disconnectionStr = TesterTcpRemoteConnection.Receive();
            }

            Message disconnection = new Message(disconnectionStr);
            Assert.IsTrue(disconnection.IsValid);
            Assert.AreEqual("Good Bye", disconnection.InnerMessage);
            Assert.AreEqual(Command.Disconnection, disconnection.Command);

            this.TesterTcpRemoteConnection.TcpClient.Close();

            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            // creation of the connection
            var connect = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, OnMessageReceived, "Test");
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

            this.TesterTcpRemoteConnection = new TcpRemoteConnection(connectionRequest.Result, this.Separator);

            var remoteTester = new TcpRemoteConnection(connectionRequest.Result, this.Separator);
            Message message = null;

            // Wait until the effective connection request arrives to the tester
            while (message == null)
            {
                string messageStr = remoteTester.Receive();
                if (!String.IsNullOrWhiteSpace(messageStr))
                {
                    message = new Message(messageStr);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            remoteTester.Send(new Message(Command.ConnectionGranted, "Ok").ToString());

            connect.Wait();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.TesterListener.Stop();
            this.Tested.Dispose();
            this.TesterTcpRemoteConnection.TcpClient.Close();
        }
    }
}
