using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpSender : ISender
    {
        #region properties

        private UdpClient Sender { get; set; }

        #endregion

        #region constructors

        public UdpSender(UdpClient udpClient)
        {
            this.Sender = udpClient;
        }

        #endregion

        #region methods

        public void Send(string message, IRemoteConnection destination)
        {
#if WAN
            // In WAN mode, we simulate the lost of packages
            // 1 package in 5 is lost

            var random = new Random();

            if (random.Next(5) == 0)
            {
                Console.WriteLine(@"/!\ Package lost /!\");
            }
            else
            {
#endif
                var datagram = Encoding.ASCII.GetBytes(message);

                this.Sender.Send(datagram, datagram.Length, new IPEndPoint(IPAddress.Parse(destination.IPAddress), destination.Port));
#if WAN
            }
#endif
        }

        #endregion

        #region IDisposable support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Sender.Close();
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
