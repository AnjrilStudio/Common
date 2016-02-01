using Anjril.Common.Network.UdpImpl.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Anjril.Common.Network.UdpImpl
{
    public class UdpSocket : ISocket
    {
        #region private properties

        private UdpClient InternalUdpClient { get; set; }
        private IReceiver Receiver { get; set; }
        private ISender Sender { get; set; }
        private UdpSocketState State { get; set; }
        private Timer Timer { get; set; }

        #endregion

        #region properties

        public int ListeningPort { get { return this.Receiver.ListeningPort; } }
        public bool IsListening { get { return this.Receiver.IsListening; } }

        #endregion

        #region events

        public event MessageHandler OnConnection;
        public event MessageHandler OnConnected;
        public event MessageHandler OnConnectionFailed;
        public event MessageHandler OnReceive;

        #endregion

        #region constructors

        public UdpSocket(int port, MessageHandler onConnection, MessageHandler onConnected, MessageHandler onConnectionFailed, MessageHandler onReceive)
        {
            this.InternalUdpClient = new UdpClient(port);

            this.Receiver = new UdpReceiver(this.InternalUdpClient, MessageReceived);
            this.Sender = new UdpSender(this.InternalUdpClient);

            this.OnConnection += onConnection;
            this.OnConnected += onConnected;
            this.OnConnectionFailed += onConnectionFailed;
            this.OnReceive += onReceive;

            this.State = new UdpSocketState();
            this.Timer = new Timer(Update, this.State, 0, 100); // TODO : parameterize this value
        }

        #endregion

        #region methods

        public void Connect(IRemoteConnection pair)
        {
            throw new NotImplementedException();

            //var message = "CONNECT";

            //var acquittal = this.SendWithAcquittal(message, pair);

            //var connectionRequest = new ConnectionRequest
            //{
            //    Message = acquittal,
            //    RemoteConnection = pair,
            //    ShipmentDate = acquittal.ShipmentDate
            //};

            //this.State.PendingConnections.Add(connectionRequest);
        }

        public void StartListening()
        {
            this.Receiver.StartListening();
        }

        public void StopListening()
        {
            this.Dispose();
        }

        public void Send(string message, IRemoteConnection destination)
        {
            var udpRemote = destination.ToUdpRemoteConnection();

            ulong nextId = udpRemote.NextSendingId;

            var msg = new Message(nextId, Command.Other, message);

            this.SendWithAcquittal(msg, destination);

            udpRemote.IncrementSendingId();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Callback called for every message received by the <see cref="Receiver"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void MessageReceived(IRemoteConnection sender, string message)
        {
            var msg = new Message(message);

            if(msg.Command != Command.Acquittal)
                this.SendAcquittal(msg, sender);

            var connectedRemote = this.GetConnectedRemote(sender).ToUdpRemoteConnection();

            switch(msg.Command)
            {
                case Command.Acquittal:
                    var acquittal = this.State.PendingAcquittals.SingleOrDefault(a => a.Message.Id == msg.Id);

                    if (acquittal != null)
                        this.State.PendingAcquittals.Remove(acquittal);
                    break;
                case Command.Connect:
                    if (connectedRemote == null)
                    {
                        // TODO : manage connection denied
                        sender.ToUdpRemoteConnection().Sender = this;
                        this.State.RemoteConnections.Add(sender);
                        
                        if (this.OnConnection != null) this.OnConnection(connectedRemote, msg.InnerMessage);
                    }
                    break;
                case Command.ConnectionGranted:
                    if (this.OnConnected != null) this.OnConnected(connectedRemote, msg.InnerMessage);
                    break;
                case Command.ConnectionRefused:
                    if (this.OnConnectionFailed != null) this.OnConnectionFailed(connectedRemote, msg.InnerMessage);
                    break;
                case Command.ConnectionNeeded:
                    // TODO
                    break;
                case Command.Ping:
                    // TODO
                    break;
                case Command.Pong:
                    // TODO
                    break;
                default:
                    //if (connectedRemote != null)
                    //{
                    if (connectedRemote.NextReceivingId == msg.Id)
                    {
                        this.DeliverMessage(msg, connectedRemote);

                        for(
                            var stackedMessage = connectedRemote.MessageStack.FirstOrDefault(m => m.Id == connectedRemote.NextReceivingId);
                            stackedMessage != null;
                            stackedMessage = connectedRemote.MessageStack.FirstOrDefault(m => m.Id == connectedRemote.NextReceivingId)
                        )
                        {
                            this.DeliverMessage(stackedMessage, connectedRemote);
                        }
                    }
                    else
                    {
                        connectedRemote.MessageStack.Add(msg);
                    }
                    //}
                    //else
                    //{
                    //    // TODO : CONNECTIONNEED
                    //}
                    break;
            }
        }

        /// <summary>
        /// Send the acquittal for a newly received message
        /// </summary>
        /// <param name="message">the message to acquit</param>
        /// <param name="sender">the sender of the original message</param>
        private void SendAcquittal(Message message, IRemoteConnection sender)
        {
            var msg = new Message(message.Id, Command.Acquittal, String.Empty);

            this.Sender.Send(msg.ToString(), sender);
        }

        /// <summary>
        /// Send a message to its addressee, add the acquittal into the socket state and returns it
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destination"></param>
        /// <returns>the acquittal added into the socket state</returns>
        private AcquittalRequest SendWithAcquittal(Message message, IRemoteConnection destination)
        {
            var acquittal = new AcquittalRequest
            {
                Message = message,
                RemoteConnection = destination,
                ShipmentDate = DateTime.Now
            };

            this.Sender.Send(message.ToString(), destination);

            this.State.PendingAcquittals.Add(acquittal);

            return acquittal;
        }

        /// <summary>
        /// Send a message to its addressee without expecting any acquittal
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destination"></param>
        /// <returns>the acquittal added into the socket state</returns>
        private void SendWithoutAcquittal(Message message, IRemoteConnection destination)
        {
            this.Sender.Send(message.ToString(), destination);
        }

        /// <summary>
        /// Gets whether it exists the connected remote from the incomming remote connection, null otherwise
        /// </summary>
        /// <param name="remoteConnection">the incomming remote connection</param>
        /// <returns></returns>
        private IRemoteConnection GetConnectedRemote(IRemoteConnection remoteConnection)
        {
            return this.State.RemoteConnections.SingleOrDefault(remote => remote.IPAddress == remoteConnection.IPAddress && remote.Port == remoteConnection.Port);
        }

        /// <summary>
        /// Gets a value that indicates whether the incomming remote connection is already connected to the socket
        /// </summary>
        /// <param name="remoteConnection">the incomming remote connection</param>
        /// <returns></returns>
        private bool IsAlreadyConnected(IRemoteConnection remoteConnection)
        {
            return this.GetConnectedRemote(remoteConnection) != null;
        }

        /// <summary>
        /// Callback called periodically to update the socket state (re-send unacquitted messages, ping connected remotes, etc)
        /// </summary>
        /// <param name="state"></param>
        private void Update(object state)
        {
            var now = DateTime.Now;

            #region acquittal management

            var intervalBeforeResend = new TimeSpan(1000); // TODO : parameterize this parameter

            var messageToResend = this.State.PendingAcquittals.Where(a => now - a.ShipmentDate > intervalBeforeResend).ToList();

            foreach(var acquittal in messageToResend)
            {
                acquittal.ShipmentDate = now;
                this.SendWithoutAcquittal(acquittal.Message, acquittal.RemoteConnection);
            }

            #endregion
        }

        /// <summary>
        /// Raises the OnReceive event to effectively deliver a message
        /// </summary>
        /// <param name="message">The message to deliver</param>
        /// <param name="recipient">The recipient of the message</param>
        private void DeliverMessage(Message message, UdpRemoteConnection recipient)
        {
            if (this.OnReceive != null)
            {
                this.OnReceive(recipient, message.InnerMessage);
            }

            recipient.IncrementReceivingId();
            recipient.MessageStack.Remove(message);
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
                    this.Receiver.Dispose();
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
