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

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var completedFiles = await _mediator.Send(new GetCompletedFilesQuery());
				var newFiles = completedFiles.Where(x => x.File.IsNew());
				var changedFiles = completedFiles.Where(x => !x.File.IsNew()).OrderBy(x => x.File.Version);
				await AssembleNewFiles(changedFiles);
				await ApplyChangedToExistingFiles(changedFiles);

				await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
			}
		}

		private async Task AssembleNewFiles(IOrderedEnumerable<CompletedFileDto> changedFiles)
		{
			var runningTasks = new List<Task>();
			var semaphore = new SemaphoreSlim(10); //todo create setting for this

			foreach (var completedFile in changedFiles)
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

		private async Task ApplyChangedToExistingFiles(IOrderedEnumerable<CompletedFileDto> changedFiles)
		{
			foreach (var changedFile in changedFiles)
			{
				await _fileAssemblerServiceFactory.Get().ProcessFile(changedFile.File, changedFile.EofMessage);
			}
		}
	}
}
