using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public class PathBuilder
    {
        public PathBuilder(IConfiguration configuration)
        {
            BasePath = configuration[Constants.ReplicateBasePath];
        }

		public string BasePath { get; }

		public string BuildPath(string relativePath)
        {
            return System.IO.Path.Combine(BasePath, relativePath??"");
        }
    }
}
