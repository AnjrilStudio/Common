using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Anjril.Common.Network
{
    public class Socket : IReceiver, ISender
    {
        #region properties

        private IReceiver Receiver { get; set; }
        private ISender Sender { get; set; }

        public int ListeningPort { get { return this.Receiver.ListeningPort; } }

        #endregion

        #region events

        public event ReceiveHandler OnReceive;

        #endregion
        
        #region constructors

        public Socket(IReceiver receiver, ISender sender, ReceiveHandler handler)
        {
            this.OnReceive += handler;

            this.Sender = sender;

            this.Receiver = receiver;
            this.Receiver.OnReceive += MessageReceived;
        }

        #endregion

        #region methods

        public void Send(string message, RemoteConnection destination)
        {
            // TODO : manage acquittement

            this.Sender.Send(message, destination);
        }

        public void StartListening()
        {
            var thread = new Thread(new ThreadStart(this.Receiver.StartListening));
            thread.Start();
        }

        #endregion

        #region private methods

        private void MessageReceived(RemoteConnection sender, string message)
        {
            // TODO : manage acquittement

            if (this.OnReceive != null)
            {
                this.OnReceive(sender, message);
            }
        }

        #endregion
    }
}
