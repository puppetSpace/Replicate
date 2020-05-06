using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class Constants
    {
        public const string ReplicateBasePath = "BaseFolder";
        public const string FolderCrawlTriggerInterval = "TriggerIntervalFolderCrawl";
        public const string FileAssemblyTriggerInterval = "TriggerIntervalFileAssembly";
		public const string RetryTriggerInterval = "TriggerIntervalRetry";
        public const string FileSplitSizeOfChunksInBytes = "SizeOfChunksInBytes";
        public const string ConcurrentFileDisassemblyJobs = "ConcurrentFileDisassemblyJobs";
        public const string ConcurrentFileAssemblyJobs = "ConcurrentFileAssemblyJobs";
	}
}
