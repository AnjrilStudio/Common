namespace Anjril.Common.Network.UdpImpl.Internal
{
    internal enum Command
    {
        Acknowledgment,
        Connect,
        ConnectionGranted,
        ConnectionRefused,
        ConnectionNeeded,
        Ping,
        Pong,
        Other,
        AlreadyConnected
    }
}
