namespace Anjril.Common.Network.UdpImpl.State
{
    using Anjril.Common.Network.UdpImpl.Internal;

    internal class AcquittalRequest : BaseRequest
    {
        /// <summary>
        /// The message sended to the remote connection
        /// </summary>
        public Message Message { get; set; }
    }
}
