namespace Anjril.Common.Network.UdpImpl.Internal
{
    internal enum Command
    {
        Acquittal,
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
