namespace Anjril.Common.Network.UdpTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net;
    using UdpImpl.Internal;

    [TestClass]
    public class UdpRemoteConnectionTest
    {
        #region properties

        private List<UdpRemoteConnection> ListEquals { get; set; }
        private List<UdpRemoteConnection> ListNotEquals { get; set; }

        #endregion

        [TestInitialize]
        public void MyClassInitialize()
        {
            // List of equals UdpRemoteConnection
            ListEquals = new List<UdpRemoteConnection>
            {
                new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000), null),
                new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000), null)
            };

            // List of not equals UdpRemoteConnection with first list
            ListNotEquals = new List<UdpRemoteConnection>
            {
                new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 16000), null),
                new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse("192.168.0.10"), 15000), null),
                new UdpRemoteConnection(new IPEndPoint(IPAddress.Parse("192.168.0.10"), 16000), null),
            };
        }

        [TestMethod]
        public void TestEquals()
        {
            for(int i = 0; i < this.ListEquals.Count; i++)
            {
                var remote = this.ListEquals[i];

                for (int j = 0; j < this.ListEquals.Count; j++)
                {
                    var remoteEqual = this.ListEquals[j];

                    Assert.AreEqual(remote, remoteEqual, string.Format("The {0}° element of the equal list is not equal to the {1}° element.", i, j));
                }

                for (int j = 0; j < this.ListNotEquals.Count; j++)
                {
                    var remoteNotEqual = this.ListNotEquals[j];

                    Assert.AreNotEqual(remote, remoteNotEqual, string.Format("The {0}° element of the not equal list is equal to the {1}° element.", i, j));
                }
            }
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            for (int i = 0; i < this.ListEquals.Count; i++)
            {
                var remote = this.ListEquals[i];

                for (int j = 0; j < this.ListEquals.Count; j++)
                {
                    var remoteEqual = this.ListEquals[j];

                    Assert.AreEqual(remote.GetHashCode(), remoteEqual.GetHashCode(), string.Format("The hash code of the {0}° element of the equal list is not equal to the hash code of the {1}° element.", i, j));
                }

                for (int j = 0; j < this.ListNotEquals.Count; j++)
                {
                    var remoteNotEqual = this.ListNotEquals[j];

                    Assert.AreNotEqual(remote.GetHashCode(), remoteNotEqual.GetHashCode(), string.Format("The hash code of the {0}° element of the not equal list is equal to the hash code of the {1}° element.", i, j));
                }
            }
        }
    }
}
