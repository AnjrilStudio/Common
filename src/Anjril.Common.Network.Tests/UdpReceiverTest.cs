using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using Anjril.Common.Network.UdpImpl;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Anjril.Common.Network.Tests
{
    [TestClass]
    public class UdpReceiverTest
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

        public IReceiver Receiver { get; set; }
        public UdpClient Sender { get; set; }
        public int ListeningPort { get; private set; }
        public int SendingPort { get; private set; }

        public string MessageReceived { get; set; }
        public IRemoteConnection MessageOrigin { get; set; }

        public UdpReceiverTest()
        {
            this.ListeningPort = 15000;
            this.SendingPort = 16000;
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.Receiver = new UdpReceiver(new UdpClient(this.ListeningPort), OnMessageReceived);
            this.Sender = new UdpClient(this.SendingPort);

            this.MessageReceived = null;
        }

        [TestMethod]
        public void TestReceive()
        {
            var message = "This is a test";

            this.Receiver.StartListening();

            var msg = Encoding.ASCII.GetBytes(message);

            this.Sender.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.ListeningPort));

            while(String.IsNullOrEmpty(this.MessageReceived))
            {
                Thread.Sleep(10);
            }

            Assert.AreEqual(message, this.MessageReceived, "The received message is different from the sended one.");
            Assert.AreEqual("127.0.0.1", this.MessageOrigin.IPAddress, "The sender didn't send its message from the expected address.");
            Assert.AreEqual(this.SendingPort, this.MessageOrigin.Port, "The sender didn't send its message from the expected port.");
        }

        [TestMethod]
        public void TestStopListening()
        {
            this.Receiver.StartListening();

            // Wait 0,2 second to ensure that the receiver is listening
            Thread.Sleep(200);

            this.Receiver.StopListening();

            // Wait 0,2 second to ensure that the receiver stopped listening
            Thread.Sleep(200);

            var msg = Encoding.ASCII.GetBytes("This is a test");

            this.Sender.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.ListeningPort));

            // Wait a little bit to ensure that no message has been received before ending the test
            Thread.Sleep(500);

            Assert.IsTrue(String.IsNullOrEmpty(this.MessageReceived), "A message has been received after the call to StopListening.");
        }

        private void OnMessageReceived(IRemoteConnection sender, string message)
        {
            this.MessageOrigin = sender;
            this.MessageReceived = message;
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.Receiver.Dispose();
            this.Sender.Close();
        }
    }
}
