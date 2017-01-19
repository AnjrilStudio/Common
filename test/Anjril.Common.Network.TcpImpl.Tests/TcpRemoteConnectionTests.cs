using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using Anjril.Common.Network.TcpImpl;
using System.Threading;
using System.Text;
using Anjril.Common.Network.TcpImpl.Internals;

namespace Anjril.Common.Network.TcpTests
{
    [TestClass]
    public class TcpRemoteConnectionTests
    {
        private string Separator { get; set; }
        private TcpRemoteConnection Tested { get; set; }
        private TcpClient Tester { get; set; }
        private TcpListener TesterListener { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Separator = "<sep>";

            var testedEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 16000);
            var testedTcpClient = new TcpClient(testedEndPoint);
            this.Tested = new TcpRemoteConnection(testedTcpClient);

            var testerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            this.TesterListener = new TcpListener(testerEndPoint);
            this.TesterListener.Start();

            var testerAsync = this.TesterListener.AcceptTcpClientAsync();

            testedTcpClient.Connect(testerEndPoint);

            testerAsync.Wait();

            this.Tester = testerAsync.Result;
        }

        [TestMethod]
        public void TestSend()
        {
            var message = "This is a test";
            this.Tested.Send(message);

            // Wait 0.2s to ensure the message is fully delivered
            Thread.Sleep(200);

            Assert.AreNotEqual(0, this.Tester.Available);
            byte[] buffer = new byte[this.Tester.Available];

            this.Tester.GetStream().Read(buffer, 0, buffer.Length);

            var messageReceived = Encoding.ASCII.GetString(buffer);

            Assert.AreEqual(Command.Message + "|" + message + this.Separator, messageReceived);
        }

        [TestMethod]
        public void TestReceive()
        {
            var messageReceived = this.Tested.Receive();

            Assert.IsNull(messageReceived);

            var message = new Message(Command.Message, "This is a test");

            byte[] buffer = Encoding.ASCII.GetBytes(message.ToString() + this.Separator);
            this.Tester.Client.Send(buffer);

            // Wait 0.2s to ensure the message is fully delivered
            Thread.Sleep(200);

            messageReceived = this.Tested.Receive();

            AreMessageEquals(messageReceived, message);

            messageReceived = this.Tested.Receive();

            Assert.IsNull(messageReceived);

            message = new Message(Command.Message, message.InnerMessage + 1);
            Message message2 = new Message(Command.Message, message.InnerMessage + 2);

            buffer = Encoding.ASCII.GetBytes(message.ToString() + this.Separator + message2.ToString() + this.Separator + message.ToString());
            this.Tester.Client.Send(buffer);

            // Wait 0.2s to ensure the message is fully delivered
            Thread.Sleep(200);

            messageReceived = this.Tested.Receive();

            AreMessageEquals(messageReceived, message);

            messageReceived = this.Tested.Receive();

            AreMessageEquals(messageReceived, message2);

            messageReceived = this.Tested.Receive();

            Assert.IsNull(messageReceived);

            buffer = Encoding.ASCII.GetBytes(this.Separator);
            this.Tester.Client.Send(buffer);

            // Wait 0.2s to ensure the message is fully delivered
            Thread.Sleep(200);

            messageReceived = this.Tested.Receive();

            AreMessageEquals(messageReceived, message);

            messageReceived = this.Tested.Receive();

            Assert.IsNull(messageReceived);
        }

        private static void AreMessageEquals(Message messageReceived, Message message)
        {
            Assert.IsTrue(messageReceived.IsValid);
            Assert.AreEqual(message.Command, messageReceived.Command);
            Assert.AreEqual(message.InnerMessage, messageReceived.InnerMessage);
        }

        [TestMethod]
        public void TestProperties()
        {
            Assert.AreEqual((this.TesterListener.LocalEndpoint as IPEndPoint).Port, this.Tested.Port);
            Assert.AreEqual((this.TesterListener.LocalEndpoint as IPEndPoint).Address.ToString(), this.Tested.IPAddress);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tester.Dispose();
            this.Tested.TcpClient.Dispose();
            this.TesterListener.Stop();
        }
    }
}
