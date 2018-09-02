using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.FileChunks
{
    internal class FileChunkObserver : IObserver<FileChunk>
    {
        private static readonly ILogger _logger = LoggerFactory.Get<FileChunkObserver>();
        private IRepositoryFactory _repository;

        public FileChunkObserver(IRepositoryFactory repository)
        {
            _repository = repository;
        }

        public void OnCompleted()
        {
            _logger.Trace("FilechunkObserver OnCompleted called");
            //todo
        }

        public void OnError(Exception error)
        {
            _logger.Trace("FilechunkObserver OnError called");
            //todo
        }

        public void OnNext(FileChunk value)
        {
            _logger.Trace("FilechunkObserver OnNext called");
            //todo
        }
    }
}
