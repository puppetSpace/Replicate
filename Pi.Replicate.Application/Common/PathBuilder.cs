using Microsoft.Extensions.Configuration;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{
    public class PathBuilder
    {
        public PathBuilder(IConfiguration configuration)
        {
            BasePath = configuration[Constants.ReplicateBasePath];
        }

		public string BasePath { get; }

		public string BuildPath(Folder folder)
        {
            return System.IO.Path.Combine(BasePath, folder?.Name??"");
        }

        public string BuildPath(File file)
        {
            return System.IO.Path.Combine(BasePath, file?.Path??"");
        }
    }
}
