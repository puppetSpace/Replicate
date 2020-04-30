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
        private readonly Startup _startupCleanup;
        private readonly FolderWorker _folderWorker;
        private readonly FileProcessWorker _fileProcessWorker;
        private readonly FileExportWorker _fileExportWorker;
        private readonly ChunkExportWorker _chunkExportWorker;

        public App(Startup startupCleanup, FolderWorker folderWorker, FileProcessWorker filePreExportWorker, FileExportWorker fileExportWorker, ChunkExportWorker chunkExportWorker)
        {
            _startupCleanup = startupCleanup;
            _folderWorker = folderWorker;
            _fileProcessWorker = filePreExportWorker;
            _fileExportWorker = fileExportWorker;
            _chunkExportWorker = chunkExportWorker;
        }

        public async Task Run()
        {
            var cancellationToken = new CancellationTokenSource();
            await _startupCleanup.Initialize();
			_folderWorker.DoWork(cancellationToken.Token);
			_fileExportWorker.DoWork(cancellationToken.Token);
			_fileProcessWorker.DoWork(cancellationToken.Token);
			_chunkExportWorker.DoWork(cancellationToken.Token);

			await Task.Delay(Timeout.Infinite);
        }
    }
}
