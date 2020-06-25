using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class ByteDisplayConverter
    {

        public static string Convert(long bytes) => bytes switch
        {
            var x when x >= 1000000000 => $"{x / 1000000000}GB",
            var x when x >= 1000000 => $"{x / 1000000}MB",
            var x when x >= 1000 => $"{x / 1000}KB",
            _=> $"{bytes} bytes"
        };
    }
}
