using Pi.Replicate.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Console
{
    public class App
    {
        private readonly FolderWorker _folderWorker;
        private readonly FilePreExportWorker _filePreExportWorker;
        private readonly FileExportWorker _fileExportWorker;
        private readonly ChunkExportWorker _chunkExportWorker;

        public App(FolderWorker folderWorker, FilePreExportWorker filePreExportWorker, FileExportWorker fileExportWorker, ChunkExportWorker chunkExportWorker)
        {
            _folderWorker = folderWorker;
            _filePreExportWorker = filePreExportWorker;
            _fileExportWorker = fileExportWorker;
            _chunkExportWorker = chunkExportWorker;
        }

        public async Task Run()
        {
            var cancellationToken = new CancellationTokenSource();
            _folderWorker.DoWork(cancellationToken.Token);
            _filePreExportWorker.DoWork(cancellationToken.Token);
            _fileExportWorker.DoWork(cancellationToken.Token);
            _chunkExportWorker.DoWork(cancellationToken.Token);

            await Task.Delay(Timeout.Infinite);
        }
    }
}
