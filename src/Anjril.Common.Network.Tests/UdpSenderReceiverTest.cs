using Anjril.Common.Network.UdpImpl.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anjril.Common.Network.Tests
{
    [TestClass]
    public class UdpSenderTest
    {
        private UdpSenderReceiver Sender { get; set; }
        private IList<Message> MessageReceived { get; set; }
        private UdpClient Receiver { get; set; }
        private UdpHelper UdpHelper { get; set; }

        private int SenderPort { get; set; }
        private int ReceiverPort { get; set; }

        public UdpSenderTest()
        {
            this.SenderPort = 15000;
            this.ReceiverPort = 16000;
        }

        private void onReceive(IRemoteConnection sender, Message message)
        {
            throw new NotImplementedException();
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.Receiver = new UdpClient(this.ReceiverPort);
            this.Sender = new UdpSenderReceiver(new UdpClient(this.SenderPort), onReceive);

            this.UdpHelper = new UdpHelper(this.Receiver);

            this.MessageReceived = new List<Message>();
        }

        [TestMethod]
        public void TestAcquittal()
        {
            var message = new Message(1, Command.Other, "Yolo");
            this.Sender.Send(message, new UdpRemoteConnection("127.0.0.1", this.ReceiverPort));
            this.Sender.StartListening();

            var datagram = this.Receiver.ReceiveAsync();

            for (int i = 0; i < 4; i++)
            {
                datagram.Wait();

                var response = new Message(UdpHelper.DeserializeDatagram(datagram.Result.Buffer));

                Assert.AreEqual(message.Id, response.Id);
                Assert.AreEqual(message.InnerMessage, response.InnerMessage);
                Assert.AreEqual(message.Command, response.Command);

                datagram = this.Receiver.ReceiveAsync();
            }

            datagram.Wait();

            UdpHelper.SendMessage(new Message(1, Command.Acquittal, null), new UdpRemoteConnection("127.0.0.1", this.SenderPort));

            // We wait 0.2s to ensure the sender has received the acquittal
            Thread.Sleep(200);

            datagram = this.Receiver.ReceiveAsync();

            // We wait 1s to ensure the sender has stopped sending message
            datagram.Wait(1000);

            Assert.IsFalse(datagram.IsCompleted);
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
