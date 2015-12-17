using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using Anjril.Common.Network.UdpImpl;

namespace Anjril.Common.Network.Tests
{
    [TestClass]
    public class UdpSenderTest
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

        public ISender Sender { get; set; }
        public UdpClient Receiver { get; set; }
        public int ListeningPort { get; private set; }
        public int SendingPort { get; private set; }

        public UdpSenderTest()
        {
            this.ListeningPort = 15000;
            this.SendingPort = 16000;
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void TestInitialize()
        {
            this.Receiver = new UdpClient(this.ListeningPort);
            this.Sender = new UdpSender(new UdpClient(this.SendingPort));
        }

        [TestMethod]
        public void TestSend()
        {
            var message = "This is a test";

            var listening = this.Receiver.ReceiveAsync();

        send:
            this.Sender.Send(message, new UdpRemoteConnection("127.0.0.1", this.ListeningPort));

            listening.Wait(2000);

            // The message has been lost.
            if (!listening.IsCompleted)
                goto send;

            var receivedMessage = Encoding.ASCII.GetString(listening.Result.Buffer);

            Assert.AreEqual(message, receivedMessage, "The message received is different from the sended one.");

            var remoteEndPoint = listening.Result.RemoteEndPoint;

            Assert.AreEqual(this.SendingPort, remoteEndPoint.Port, "The sender didn't send its message from the expected port.");
            Assert.AreEqual("127.0.0.1", remoteEndPoint.Address.ToString(), "The sender didn't send its message from the expected address.");
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void TestCleanup()
        {
            this.Receiver.Dispose();
            this.Sender.Dispose();
        }
    }
}
