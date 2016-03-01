using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using Anjril.Common.Network.UdpImpl;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Anjril.Common.Network.UdpImpl.Internal;

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

        private IReceiver Receiver { get; set; }
        private UdpClient Sender { get; set; }
        private int ListeningPort { get; set; }
        private int SendingPort { get; set; }
        private Message MessageReceived { get; set; }
        private IRemoteConnection MessageOrigin { get; set; }

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

        private void OnMessageReceived(IRemoteConnection sender, Message message)
        {
            this.MessageOrigin = sender;
            this.MessageReceived = message;
        }

        [TestMethod]
        public void TestReceive()
        {
            var message = new Message(1, Command.Other, "This is a test");

            this.Receiver.StartListening();
            var aquittement = this.Sender.ReceiveAsync();

            var msg = Encoding.ASCII.GetBytes(message.ToString());

            this.Sender.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.ListeningPort));

            while(this.MessageReceived == null)
            {
                Thread.Sleep(10);
            }

            Assert.AreEqual(message.Id, this.MessageReceived.Id, "The received message's id is different from the sended one.");
            Assert.AreEqual(message.Command, this.MessageReceived.Command, "The received message's command is different from the sended one.");
            Assert.AreEqual(message.InnerMessage, this.MessageReceived.InnerMessage, "The received message's inner message is different from the sended one.");
            Assert.AreEqual("127.0.0.1", this.MessageOrigin.IPAddress, "The sender didn't send its message from the expected address.");
            Assert.AreEqual(this.SendingPort, this.MessageOrigin.Port, "The sender didn't send its message from the expected port.");

            aquittement.Wait();

            var remoteEndPoint = aquittement.Result.RemoteEndPoint;
            var response = new Message(Encoding.ASCII.GetString(aquittement.Result.Buffer));

            Assert.AreEqual(this.ListeningPort, remoteEndPoint.Port);
            Assert.AreEqual(Command.Acquittal, response.Command);
            Assert.AreEqual(message.Id, response.Id);
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

            Assert.IsTrue(this.MessageReceived == null, "A message has been received after the call to StopListening.");
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.Receiver.Dispose();
            this.Sender.Dispose();
        }
    }
}
