using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Files.Commands.AddNewFileToQueue;
using Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	public class FolderWorker : WorkerBase
	{
		private readonly int _triggerInterval;
		private readonly IMediator _mediator;
		private readonly FileCollectorFactory _fileCollectorFactory;

		public FolderWorker(IConfiguration configuration
			, IMediator mediator
			, FileCollectorFactory fileCollectorFactory)
		{
			_triggerInterval = int.TryParse(configuration["FolderCrawlTriggerInterval"], out int interval) ? interval : 10;
			_mediator = mediator;
			_fileCollectorFactory = fileCollectorFactory;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var workingThread = new Thread(async () =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var folders = await _mediator.Send(new GetFoldersToCrawlQuery());
					foreach (var folder in folders)
					{
						var collector = _fileCollectorFactory.Get(folder);

						await _mediator.Send(new AddNewFilesToQueueCommand(await collector.GetNewFiles(), folder)); //add to queue
						//await collector.GetChangedFiles(); //add to queue
					}
				}

				await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
			});
			workingThread.Start();
			return workingThread;
		}
	}
}
