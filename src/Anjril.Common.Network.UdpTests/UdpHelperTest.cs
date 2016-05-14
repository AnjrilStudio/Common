namespace Anjril.Common.Network.UdpTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpImpl.Internal;

    [TestClass]
    public class UdpHelperTest
    {
        #region properties

        private const string LOCALHOST = "127.0.0.1";
        private const int TESTER_PORT = 15000;
        private const int TESTED_PORT = 16000;

        private UdpClient Tester { get; set; }
        private UdpHelper UdpHelper { get; set; }

        #endregion

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.UdpHelper = new UdpHelper(new UdpClient(TESTED_PORT));
            this.Tester = new UdpClient(TESTER_PORT);
        }

        [TestMethod]
        public void TestUdpSend()
        {
            var testSend = this.Tester.ReceiveAsync();

            var messageSent = "This is a test!";
            var endpoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), TESTER_PORT);

            this.UdpHelper.SendMessage(messageSent, endpoint);

            testSend.Wait();

            Assert.AreEqual(messageSent, this.UdpHelper.DeserializeDatagram(testSend.Result.Buffer), "The received message is different compared to the sent one.");
            Assert.AreEqual(TESTED_PORT, testSend.Result.RemoteEndPoint.Port, "The port of the remote endpoint is not the port specified into the constructor.");
            Assert.AreEqual(LOCALHOST, testSend.Result.RemoteEndPoint.Address.ToString(), "The ip of the remote endpoint is not the ip specified into the constructor.");
        }

        [TestMethod]
        public void TestUdpReceive()
        {
            var message = "This is a test!";
            var endpoint = new IPEndPoint(IPAddress.Parse(LOCALHOST), TESTED_PORT);
            var datagram = this.UdpHelper.SerializeMessage(message);

            IPEndPoint endpointSender = null;
            var testReceive = Task.Run(() => this.UdpHelper.ReceiveMessage(out endpointSender));

            this.Tester.Send(datagram, datagram.Length, endpoint);

            testReceive.Wait();

            Assert.AreEqual(message, testReceive.Result.ToString(), "The received message is different compared to the sent one.");
            Assert.AreEqual(TESTER_PORT, endpointSender.Port, "The port of the remote endpoint is not the port specified into the constructor.");
            Assert.AreEqual(LOCALHOST, endpointSender.Address.ToString(), "The ip of the remote endpoint is not the ip specified into the constructor.");
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tester.Dispose();
            this.UdpHelper.Dispose();
        }
    }
}
