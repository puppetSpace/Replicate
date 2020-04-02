using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    //Flow
    // FolderWatcher -> FileCollector -> FileSplitter -> FileChunkUpload #TRANSMISSION# FileChunkDownload -> FileChecker -> FileAssembler
    //                       IN              IN                IN                              IN                                IN
    //                   FolderQueue      FileQueue      FileChunkQueue                   FileChunkQueue                      FileQueue
    //      OUT              OUT             OUT                                                                 OUT         
    //  FolderQueue       FileQueue      FileChunkQueue                                                       FileQueue
    internal class Constants
    {
        public const string PollDelay = "PollDelay";
        public const string FileSplitSizeOfChunksInBytes = "FileSplitSizeOfChunksInBytes";
    }
}
