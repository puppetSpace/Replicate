using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{
    public static class ByteDisplayConverter
    {

        public static string Convert(long bytes) => bytes switch
        {
            var x when x >= 1000 => $"{x / 1000} Kb",
            var x when x >= 1000000 => $"{x / 1000000} Mb",
            var x when x >= 1000000000 => $"{x / 1000000000} Gb",
            _=> $"{bytes} bytes"
        };
    }
}
