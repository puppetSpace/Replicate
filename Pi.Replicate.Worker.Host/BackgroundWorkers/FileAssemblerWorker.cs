using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Files.Queries.GetCompletedFiles;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileAssemblerWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly IMediator _mediator;
		private readonly FileAssemblerServiceFactory _fileAssemblerServiceFactory;

		public FileAssemblerWorker(IConfiguration configuration, IMediator mediator, FileAssemblerServiceFactory fileAssemblerServiceFactory)
		{
			_mediator = mediator;
			_fileAssemblerServiceFactory = fileAssemblerServiceFactory;
			_triggerInterval = int.Parse(configuration[Constants.FileAssemblyTriggerInterval]);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					var completedFilesResult = await _mediator.Send(new GetCompletedFilesQuery());
					if (completedFilesResult.WasSuccessful)
					{
						var newFiles = completedFilesResult.Data.Where(x => x.File.IsNew());
						var changedFiles = completedFilesResult.Data.Where(x => !x.File.IsNew()).OrderBy(x => x.File.Version);
						await AssembleNewFiles(newFiles);
						await ApplyChangedToExistingFiles(changedFiles);

						await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
					}

				}
			});

			th.Start();

			return Task.CompletedTask;
		}

		private async Task AssembleNewFiles(IEnumerable<CompletedFileDto> newFiles)
		{
			var runningTasks = new List<Task>();
			var semaphore = new SemaphoreSlim(10); //todo create setting for this

			foreach (var completedFile in newFiles)
			{
				runningTasks.Add(Task.Run(async () =>
				{
					await semaphore.WaitAsync();
					await _fileAssemblerServiceFactory.Get().ProcessFile(completedFile.File, completedFile.EofMessage);
					semaphore.Release();
				}));
			}
			await Task.WhenAll(runningTasks);
			runningTasks.Clear();
		}

		private async Task ApplyChangedToExistingFiles(IEnumerable<CompletedFileDto> changedFiles)
		{
			foreach (var changedFile in changedFiles)
			{
				await _fileAssemblerServiceFactory.Get().ProcessFile(changedFile.File, changedFile.EofMessage);
			}
		}
	}
}
