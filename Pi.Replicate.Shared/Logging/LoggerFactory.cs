using System;

namespace Pi.Replicate.Shared.Logging
{
    public static class LoggerFactory
    {
        //todo maybe keep cache of loggers for TE
        public static ILogger Get<TE>()
        {
            return new PiLogger<TE>();
        }
    }
}
