using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Logging
{
    internal class PiLogger<TE> : ILogger
    {
        private NLog.ILogger _logger;

        public PiLogger()
        {
            _logger = NLog.LogManager.GetLogger(typeof(TE).Name);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Error(Exception ex, string message)
        {
            _logger.Error(ex, message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }
    }
}
