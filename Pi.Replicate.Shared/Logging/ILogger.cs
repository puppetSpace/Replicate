using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Logging
{
    public interface ILogger
    {
        void Debug(string message);

        void Trace(string message);

        void Info(string message);

        void Error(Exception ex,string message);

        void Warn(string message);
    }
}
