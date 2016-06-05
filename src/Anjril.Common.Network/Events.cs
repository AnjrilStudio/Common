﻿namespace Anjril.Common.Network
{
    /// <summary>
    /// Delegate to handle incomming messages
    /// </summary>
    /// <param name="sender">The remote client sending the message</param>
    /// <param name="message">The message sent</param>
    public delegate void MessageHandler(IRemoteConnection sender, string message);

    /// <summary>
    /// Delegate to handle incomming connection requests
    /// </summary>
    /// <param name="sender">The remote client requesting a connection</param>
    /// <param name="request">The request message</param>
    /// <param name="response">The response the socket will send to this request</param>
    /// <returns>The socket accepts the connection</returns>
    public delegate bool ConnectionHandler(IRemoteConnection sender, string request, out string response);

    /// <summary>
    /// Delegate to handle the disconnection of remote connections
    /// </summary>
    /// <param name="remote">the disconnected remote</param>
    /// <param name="justification">the justification sended by the remote</param>
    public delegate void DisconnectionHandler(IRemoteConnection remote, string justification);
}
