using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Files.Commands.AddNewFiles;
using Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	//todo on start delete files with status new and changed. When theyhave this status, it means the process got shut down before chunks were made
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
					var folders = await _mediator.Send(new GetFoldersToCrawlQuery(), cancellationToken);
					foreach (var folder in folders)
					{
						var collector = _fileCollectorFactory.Get(folder);
						var newFoundFiles = await collector.GetNewFiles();
						await _mediator.Send(new AddNewFilesCommand(newFoundFiles, folder));
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
