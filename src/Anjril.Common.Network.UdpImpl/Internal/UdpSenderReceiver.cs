namespace Anjril.Common.Network.UdpImpl.Internal
{
    using State;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;

    internal class UdpSenderReceiver : ISender, IReceiver
    {
        #region properties

        public bool IsListening { get { return this.Receiver.IsListening; } }

        public int Port { get { return this.Receiver.Port; } }

        #endregion

        #region private properties

        /// <summary>
        /// UdpReceiver used internally to receive message and listen to acquittal
        /// </summary>
        private UdpReceiver Receiver { get; set; }

        /// <summary>
        /// UdpHelper used internally to send messages
        /// </summary>
        private UdpHelper UdpHelper { get; set; }

        /// <summary>
        /// Contains all the sended message waiting for an acquittal
        /// </summary>
        private IList<AcquittalRequest> MessageSended { get; set; }

        /// <summary>
        /// Timer used to periodically send message that are not yet acquitted
        /// </summary>
        private Timer Timer { get; set; }

        #endregion

        #region events

        public event InternalMessageHandler OnReceive;

        #endregion

        #region constructors

        public UdpSenderReceiver(UdpClient client, InternalMessageHandler onReceived)
        {
            this.Receiver = new UdpReceiver(client, this.OnMessageReceived);
            this.UdpHelper = new UdpHelper(client);

            this.MessageSended = new List<AcquittalRequest>();

            this.Timer = new Timer(Update, null, 0, 100); // TODO : parameterize the refresh rate
        }

        #endregion

        #region methods

        public void Send(Message message, IRemoteConnection destination)
        {
            AcquittalRequest acquittal = new AcquittalRequest
            {
                Message = message,
                RemoteConnection = destination,
                ShipmentDate = DateTime.Now
            };

            this.MessageSended.Add(acquittal);

            SendWithoutAcquittal(message, destination);
        }


        public void StartListening()
        {
            this.Receiver.StartListening();
        }

        public void StopListening()
        {
            this.Receiver.StopListening();
        }

        #endregion

        #region privates methods

        /// <summary>
        /// Method called when the internal UdpReceier receives a message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnMessageReceived(UdpRemoteConnection sender, Message message)
        {
            if (message.Command == Command.Acquittal)
            {
                var originalMessage = this.MessageSended.SingleOrDefault(acq => acq.Message.Id == message.Id && acq.RemoteConnection.Equals(sender));

                if (originalMessage != null)
                {
                    this.MessageSended.Remove(originalMessage);
                }
            }
            else if (this.OnReceive != null)
            {
                this.OnReceive(sender, message);
            }
        }

        /// <summary>
        /// Callback called periodically to send again the messages that are not yet acquitted
        /// </summary>
        /// <param name="state">this parameter is null, do not use it !</param>
        private void Update(object state)
        {
            var now = DateTime.Now;

            #region acquittal management

            var intervalBeforeResend = new TimeSpan(1000); // TODO : parameterize the time before resend parameter

            var messageToResend = this.MessageSended.Where(m => now - m.ShipmentDate > intervalBeforeResend).ToList();

            foreach (var message in messageToResend)
            {
                message.ShipmentDate = now;
                this.SendWithoutAcquittal(message.Message, message.RemoteConnection);
            }

            #endregion
        }

        #endregion

        #region internal methods

        internal void SendWithoutAcquittal(Message message, IRemoteConnection destination)
        {
            this.UdpHelper.SendMessage(message, destination);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Receiver.Dispose();
                    this.Timer.Dispose();
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
