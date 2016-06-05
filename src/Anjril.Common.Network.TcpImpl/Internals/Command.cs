namespace Anjril.Common.Network.TcpImpl.Internals
{
    enum Command
    {
        ConnectionRequest,
        ConnectionGranted,
        ConnectionFailed,
        Message,
        Disconnection,
        Disconnected
    }
}
