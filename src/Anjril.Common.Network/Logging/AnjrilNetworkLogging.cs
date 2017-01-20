namespace Anjril.Common.Network.Logging
{
#if NET35
    using global::Common.Logging;
#elif NETSTANDARD1_3
        using Microsoft.Extensions.Logging;
#endif

    public static class AnjrilNetworkLogging
    {   
#if NETSTANDARD1_3
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
#endif

        public static AnjrilLogger CreateLogger<T>()
        {
#if NET35
            var logger = LogManager.GetLogger(typeof(T));
#elif NETSTANDARD1_3
            var logger = LoggerFactory.CreateLogger<T>();
#endif

            return new AnjrilLogger(logger);
        }
    }
}
