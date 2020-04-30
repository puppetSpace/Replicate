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

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
    public class WorkerStartup
    {
        private readonly IDatabase _database;
		private readonly IMediator _mediator;
		private readonly WorkerQueueFactory _workerQueueFactory;

		public WorkerStartup(IDatabase database, IMediator mediator, WorkerQueueFactory workerQueueFactory)
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
                await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.TransmissionResult)", null);
            }            
        }
    }
}
