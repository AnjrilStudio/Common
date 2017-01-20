namespace Anjril.Common.Network.Logging
{
#if NET35
    using global::Common.Logging;
#elif NETSTANDARD1_3
    using Microsoft.Extensions.Logging;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AnjrilLogger
    {
        #region properties

#if NET35
        internal ILog Logger { get; private set; }
#elif NETSTANDARD1_3
        internal ILogger Logger { get; private set; }
#endif

        #endregion

        #region constructors
#if NET35
        internal AnjrilLogger(ILog logger)
        {
            this.Logger = logger;
        }
#elif NETSTANDARD1_3
        internal AnjrilLogger(ILogger logger)
        {
            this.Logger = logger;
        }
#endif

        #endregion

        #region public methods

        public void LogTrace(string debug)
        {
#if NET35
            this.Logger.Trace(debug);
#elif NETSTANDARD1_3
            this.Logger.LogTrace(debug);
#endif
        }

        public void LogNetwork(string network)
        {
#if NET35
            this.Logger.Debug(network);
#elif NETSTANDARD1_3
            this.Logger.LogDebug(network);
#endif
        }

        public void LogWarning(Exception e)
        {
#if NET35
            this.Logger.Warn(e.Message, e);
#elif NETSTANDARD1_3
            this.Logger.LogWarning(null, e, e.Message);
#endif
        }

        public void LogWarning(string warning)
        {
#if NET35
            this.Logger.Warn(warning);
#elif NETSTANDARD1_3
            this.Logger.LogWarning(warning);
#endif
        }

        public void LogError(Exception e)
        {
#if NET35
            this.Logger.Error(e.Message, e);
#elif NETSTANDARD1_3
            this.Logger.LogError(null, e, e.Message);
#endif
        }

        public void LogError(string error)
        {
#if NET35
            this.Logger.Error(error);
#elif NETSTANDARD1_3
            this.Logger.LogError(error);
#endif
        }

        #endregion

        #region log level

        public enum AnjrilLogLevel
        {
            Trace,
            Network,
            Warning,
            Error
        }

        #endregion
    }
}
