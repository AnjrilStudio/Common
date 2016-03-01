namespace Anjril.Common.Network.UdpImpl.Internal
{
    using System;

    internal class Message
    {
        #region properties

        /// <summary>
        /// The id of the message 
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The command carried by this message
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// The message send by the remote connection witout all the other info
        /// </summary>
        public string InnerMessage { get; set; }

        /// <summary>
        /// Gets a value that indicate whether the message format is valid
        /// </summary>
        public bool IsValid { get; set; }

        #endregion

        #region constructors

        public Message(string originalMessage)
        {
            try {
                var splitedMessage = originalMessage.Split('|');

                this.Id = UInt64.Parse(splitedMessage[0]);
                this.Command = (Command)Enum.Parse(typeof(Command), splitedMessage[1]);
                this.InnerMessage = splitedMessage[2];
                this.IsValid = true;
            }
            catch (Exception)
            {
                this.IsValid = false;
            }
        }

        public Message(ulong id, Command command, string message)
        {
            this.Id = id;
            this.Command = command;
            this.InnerMessage = message;
            this.IsValid = true;
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}", this.Id, this.Command, this.InnerMessage);
        }

        #endregion
    }
}
