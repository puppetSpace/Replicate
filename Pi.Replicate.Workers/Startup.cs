using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class Startup
    {
        private readonly IDatabase _database;
		private readonly IMediator _mediator;
		private readonly WorkerQueueFactory _workerQueueFactory;

		public Startup(IDatabase database, IMediator mediator, WorkerQueueFactory workerQueueFactory)
        {
            _database = database;
			_mediator = mediator;
			_workerQueueFactory = workerQueueFactory;
		}
        public async Task Initialize()
        {
            using (_database)
            {
				Log.Information("Deleting unprocessed files from database");
                // await _database.Execute("DELETE FROM dbo.FileChunk where fileId in (select id from dbo.[File] where status in (0,1))", null);
                // await _database.Execute("DELETE FROM dbo.[File] where status in (0,1)", null);
            }

			//Log.Information("Queing processed files that are not sent yet");
			//var outgoingQueue = _workerQueueFactory.Get<FilePreExportQueueItem>(WorkerQueueType.ToSendFiles);
			//var processedFiles = await _mediator.Send(new GetFilesByStatusQuery { Status = Domain.FileStatus.Processed });
			//foreach (var file in processedFiles)
			//	outgoingQueue.Add(new FilePreExportQueueItem<File>(file));

            
        }
    }
}
