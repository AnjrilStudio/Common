using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anjril.Common.Network.Tests.Utils
{
    public class ReceiverTester
    {
        public IReceiver Receiver { get; set; }

        public IList<Tuple<IRemoteConnection, string>> MessagedReceived { get; set; }

        public ReceiverTester(IReceiver receiver)
        {
            this.MessagedReceived = new List<Tuple<IRemoteConnection, string>>();
            this.Receiver = receiver;

            this.Receiver.OnReceive += Receiver_OnReceive;
        }

        private void Receiver_OnReceive(IRemoteConnection sender, string message)
        {
            this.MessagedReceived.Add(new Tuple<IRemoteConnection, string>(sender, message));
        }
    }
}
