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
        private readonly FileProcessForExportWorker _fileProcessForExportWorker;

        public App(FolderWorker folderWorker, FileProcessForExportWorker fileProcessForExportWorker)
        {
            _folderWorker = folderWorker;
            _fileProcessForExportWorker = fileProcessForExportWorker;
        }

        public async Task Run()
        {
            var cancellationToken = new CancellationTokenSource();
            _folderWorker.DoWork(cancellationToken.Token);
            _fileProcessForExportWorker.DoWork(cancellationToken.Token);

            await Task.Delay(Timeout.Infinite);
        }
    }
}
