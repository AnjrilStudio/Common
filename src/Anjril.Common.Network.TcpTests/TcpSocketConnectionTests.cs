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
    public class TcpSocketConnectionTests
    {
        private string Separator { get; set; }
        private Tuple<IRemoteConnection, string>[] Messages { get; set; }
        private TcpSocket Tested { get; set; }
        private TcpRemoteConnection Tester { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Separator = "<sep>";
            this.Messages = new Tuple<IRemoteConnection, string>[10];

            this.Tested = new TcpSocket(16000, this.Separator);

            var testerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            this.Tester = new TcpRemoteConnection(new TcpClient(testerEndPoint), this.Separator);
        }

        /// <summary>
        /// Method called when a message is received by the server
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

        /// <summary>
        /// Method called when a connection request is received by the server, to authorize the connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool OnConnectionRequested(IRemoteConnection sender, string request, out string response)
        {
            Assert.AreEqual("127.0.0.1", sender.IPAddress);
            Assert.AreEqual(15000, sender.Port);

            if (request == "fail")
            {
                response = "Ko";
                return false;
            }

            response = "Ok";
            return true;
        }

        /// <summary>
        /// Method called when a remote is disconnected
        /// </summary>
        /// <param name="remote">the disconnected remote</param>
        /// <param name="justification">the justification for the disconnection</param>
        private void OnDisconnection(IRemoteConnection remote, string justification)
        {
            Assert.AreEqual(15000, remote.Port);
            Assert.AreEqual("127.0.0.1", remote.IPAddress);
        }

        [TestMethod]
        public void TestConnectionRefused()
        {
            this.Tested.StartListening(OnConnectionRequested, OnMessageReceived, OnDisconnection);

            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.Tested.Port);
            this.Tester.TcpClient.Connect(endPoint);

            this.Tester.Send(new Message(Command.ConnectionRequest, "fail").ToString());

            Message message = null;
            while (message == null)
            {
                Thread.Sleep(50);

                string messageStr = this.Tester.Receive();

                if (!String.IsNullOrWhiteSpace(messageStr))
                {
                    message = new Message(messageStr);
                }
            }

            Assert.IsTrue(message.IsValid);
            Assert.AreEqual(Command.ConnectionFailed, message.Command);
            Assert.AreEqual("Ko", message.InnerMessage);
        }

        [TestMethod]
        public void TestConnectionGranted()
        {
            this.Tested.StartListening(OnConnectionRequested, OnMessageReceived, OnDisconnection);

            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.Tested.Port);
            this.Tester.TcpClient.Connect(endPoint);

            this.Tester.Send(new Message(Command.ConnectionRequest, "success").ToString());

            Message message = null;
            while (message == null)
            {
                Thread.Sleep(50);

                string messageStr = this.Tester.Receive();

                if (!String.IsNullOrWhiteSpace(messageStr))
                {
                    message = new Message(messageStr);
                }
            }

            Assert.IsTrue(message.IsValid);
            Assert.AreEqual(Command.ConnectionGranted, message.Command);
            Assert.AreEqual("Ok", message.InnerMessage);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.Tester.TcpClient.Dispose();
            this.Tested.Dispose();
        }
    }
}
