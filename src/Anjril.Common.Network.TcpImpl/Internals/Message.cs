using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anjril.Common.Network.TcpImpl.Internals
{
    internal class Message
    {
        #region properties

        public Command Command { get; private set; }
        public string InnerMessage { get; private set; }
        public bool IsValid { get; private set; }

        #endregion

        #region constructor

        public Message(string message)
        {
            var split = message.Split('|');

            this.IsValid = split.Length == 2;

            if (this.IsValid)
            {
                this.Command = (Command)Enum.Parse(typeof(Command), split[0]);
                this.InnerMessage = split[1];
            }
        }

        public Message(Command command, string message)
        {
            this.Command = command;
            this.InnerMessage = message;
            this.IsValid = true;
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return String.Format("{0}|{1}", this.Command, this.InnerMessage);
        }

        #endregion
    }
}
