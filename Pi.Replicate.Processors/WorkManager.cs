using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public sealed class WorkManager
    {
        //IN
        //FileAssembler (Consumer)
        //FileChecker (Producer)
        //FileChunkDownload (Consumer)


        //OUT
        //FileCollector (Producer-Consumer)
        //FileSplitter (Producer-Consumer)
        //FolderWatcher (Producer)
        //FileChunkUpload (Consumer)

        //Flow
        // FolderWatcher -> FileCollector -> FileSplitter -> FileChunkUpload #TRANSMISSION# FileChunkDownload -> FileChecker -> FileAssembler
        //                       IN              IN                IN                                                                IN
        //                   FolderQueue      FileQueue      FileChunkQueue                                                       FileQueue
        //      OUT              OUT             OUT                                               OUT              OUT         
        //  FolderQueue       FileQueue      FileChunkQueue                                    FileChunkQueue    FileQueue


        //Outgoing queues: FolderQueue,FileQueue & FileChunkQueue
        //InComing queues: FileChunkQueue & FileQueue

    }
}
