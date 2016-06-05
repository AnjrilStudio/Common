namespace Anjril.Common.Network.UdpTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpImpl.Internal;
    using UdpImpl.Internal.Exceptions;

    [TestClass]
    public class BasicSocketHelperTest
    {
        #region properties

        private const string LOCALHOST = "127.0.0.1";
        private const int TESTER_PORT = 15000;
        private const int TESTED_PORT = 16000;

        private UdpHelper Tester { get; set; }
        private BasicUdpSocketHelper SocketHelper { get; set; }

        private Message MessageReceived { get; set; }
        private UdpRemoteConnection Sender { get; set; }

        #endregion

        [TestInitialize]
        public void MyTestInitialize()
        {
            var udpClient = new UdpClient(TESTED_PORT);
            this.SocketHelper = new BasicUdpSocketHelper(udpClient, OnMessageReceived);

            udpClient = new UdpClient(TESTER_PORT);
            this.Tester = new UdpHelper(udpClient);
        }

        private void OnMessageReceived(UdpRemoteConnection sender, Message message)
        {
            this.MessageReceived = message;
            this.Sender = sender;
        }

        [TestMethod]
        public void TestSocketListening()
        {
            var initialMessage = new Message(0, Command.Other, "This is a test");
            var destination = new IPEndPoint(IPAddress.Parse(LOCALHOST), TESTED_PORT);

            this.Tester.SendMessage(initialMessage.ToString(), destination);

            // Waiting 200 ms to ensure no message has arrived
            Thread.Sleep(200);

            Assert.IsNull(this.MessageReceived);
            Assert.IsNull(this.Sender);

            this.SocketHelper.StartListening();

            //this.Tester.SendMessage(initialMessage.ToString(), destination);

            // Waiting 200 ms to ensure the message is arrived
            Thread.Sleep(200);

            Assert.IsNotNull(this.MessageReceived);
            Assert.IsNotNull(this.Sender);

            Assert.AreEqual(initialMessage.Id, this.MessageReceived.Id);
            Assert.AreEqual(initialMessage.Command, this.MessageReceived.Command);
            Assert.AreEqual(initialMessage.InnerMessage, this.MessageReceived.InnerMessage);

            try
            {
                this.SocketHelper.StartListening();
                Assert.Fail();
            }
            catch (SocketHelperAlreadyListeningException)
            { }

            this.SocketHelper.StopListening();

            this.MessageReceived = null;
            this.Sender = null;

            this.Tester.SendMessage(initialMessage.ToString(), destination);

            // Waiting 200 ms to ensure no message has arrived
            Thread.Sleep(200);

            Assert.IsNull(this.MessageReceived);
            Assert.IsNull(this.Sender);

            this.SocketHelper.StartListening();

            this.Tester.SendMessage(initialMessage.ToString(), destination);

            // Waiting 200 ms to ensure the message is arrived
            Thread.Sleep(200);

            Assert.IsNotNull(this.MessageReceived);
            Assert.IsNotNull(this.Sender);

            Assert.AreEqual(initialMessage.Id, this.MessageReceived.Id);
            Assert.AreEqual(initialMessage.Command, this.MessageReceived.Command);
            Assert.AreEqual(initialMessage.InnerMessage, this.MessageReceived.InnerMessage);
        }

        [TestMethod]
        public void TestSocketSend()
        {
            var messageReceived = Task.Run(() =>
            {
                var sender = new IPEndPoint(IPAddress.Any, 0);
                var ack = this.Tester.ReceiveMessage(out sender);

                return new { Sender = sender, Msg = ack };
            });

            var endPoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), TESTER_PORT);
            var initialMessage = new Message(0, Command.Other, "This is a test.");
            this.SocketHelper.Send(initialMessage, new UdpRemoteConnection(endPoint, this.SocketHelper));

            messageReceived.Wait();

            var message = new Message(messageReceived.Result.Msg);
            var origin = messageReceived.Result.Sender;

            Assert.IsTrue(message.IsValid);
            Assert.AreEqual(initialMessage.Id, message.Id);
            Assert.AreEqual(initialMessage.Command, message.Command);
            Assert.AreEqual(initialMessage.InnerMessage, message.InnerMessage);
        }

        // Test send with acquittal
        //[TestMethod]
        //public void TestSocketSend()
        //{
        //    var initialMessage = new Message(0, Command.Other, "This is a test");
        //    var destination = new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse(LOCALHOST), TESTER_PORT), this.SocketHelper);

        //    var listening = Task.Run(() =>
        //    {
        //        var messagesReceived = new List<Message>();

        //        var sender = new IPEndPoint(IPAddress.Any, 0);

        //        for (int i = 0; i < 5; i++)
        //        {
        //            var message = new Message(this.Tester.ReceiveMessage(out sender));

        //            messagesReceived.Add(message);

        //            Assert.AreEqual(initialMessage.Id, message.Id);
        //            Assert.AreEqual(initialMessage.InnerMessage, message.InnerMessage);
        //            Assert.AreEqual(initialMessage.Command, message.Command);
        //        }

        //        var response = new Message(initialMessage.Id, Command.Acknowledgment, string.Empty);

        //        var nextMessage = Task.Run(() => this.Tester.ReceiveMessage(out sender));

        //        nextMessage.Wait(500);

        //        Assert.IsFalse(nextMessage.IsCompleted);
        //    });

        //    this.SocketHelper.StartListening();

        //    this.SocketHelper.Send(initialMessage, destination);

        //    //this.SocketHelper.
        //}

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tester.Dispose();
            this.SocketHelper.Dispose();
        }
    }
}
