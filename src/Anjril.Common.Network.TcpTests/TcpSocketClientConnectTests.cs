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

namespace Anjril.Common.Network.TcpTests
{
    [TestClass]
    public class TcpSocketClientConnectTests
    {
        private string Separator { get; set; }
        private TcpSocketClient Tested { get; set; }
        private TcpListener TesterListener { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Separator = "<sep>";

            this.Tested = new TcpSocketClient(16000, this.Separator);

            var testerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            this.TesterListener = new TcpListener(testerEndPoint);

            this.TesterListener.Start();
        }

        [TestMethod]
        public void TestConnectOk()
        {
            Task<TcpClient> connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            var test = Task.Run(() =>
            {
                var response = this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");

                Assert.AreEqual("Ok", response);
                Assert.IsTrue(this.Tested.IsConnected);
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

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

            Assert.AreEqual(Command.ConnectionRequest, message.Command);
            Assert.AreEqual("Yolo", message.InnerMessage);

            remoteTester.Send(new Message(Command.ConnectionGranted, "Ok").ToString());

            // we wait until the test ends
            test.Wait();

            connectionRequest.Result.Dispose();
        }

        [TestMethod]
        public void TestConnectTimeout()
        {
            Task<TcpClient> connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual(TypeConnectionFailed.Timeout, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

            // We do nothing until the test end
            test.Wait();

            connectionRequest.Result.Dispose();
        }

        [TestMethod]
        public void TestConnectRefused()
        {
            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual("Ko", e.Message);
                    Assert.AreEqual(TypeConnectionFailed.ConnectionRefused, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

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

            Assert.AreEqual(Command.ConnectionRequest, message.Command);
            Assert.AreEqual("Yolo", message.InnerMessage);

            remoteTester.Send(new Message(Command.ConnectionFailed, "Ko").ToString());

            test.Wait();

            connectionRequest.Result.Dispose();
        }

        [TestMethod]
        public void TestConnectKo1()
        {
            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual(TypeConnectionFailed.Other, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

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

            Assert.AreEqual(Command.ConnectionRequest, message.Command);
            Assert.AreEqual("Yolo", message.InnerMessage);

            remoteTester.Send(new Message(Command.Message, "TROLOLO").ToString());

            test.Wait();

            connectionRequest.Result.Dispose();
        }

        [TestMethod]
        public void TestConnectKo2()
        {
            var connectionRequest = this.TesterListener.AcceptTcpClientAsync();

            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual(TypeConnectionFailed.InvalidResponse, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // Wait until the TCP connection request arrives to the tester
            connectionRequest.Wait();

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

            Assert.AreEqual(Command.ConnectionRequest, message.Command);
            Assert.AreEqual("Yolo", message.InnerMessage);

            remoteTester.Send("TROLOLO");

            test.Wait();

            connectionRequest.Result.Dispose();
        }

        [TestMethod]
        public void TestConnectKo3()
        {
            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 15000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual(TypeConnectionFailed.Timeout, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // we wait until the test ends
            test.Wait();
        }

        [TestMethod]
        public void TestConnectKo4()
        {
            var test = Task.Run(() =>
            {
                try
                {
                    this.Tested.Connect("127.0.0.1", 17000, null, "Yolo");
                    Assert.Fail();
                }
                catch (ConnectionFailedException e)
                {
                    Assert.IsFalse(this.Tested.IsConnected);
                    Assert.AreEqual(TypeConnectionFailed.SocketUnreachable, e.TypeErreur);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            });

            // we wait until the test ends
            test.Wait();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            this.TesterListener.Stop();
            this.Tested.Dispose();
        }
    }
}
