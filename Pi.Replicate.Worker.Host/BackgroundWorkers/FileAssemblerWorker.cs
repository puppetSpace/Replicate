using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileAssemblerWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly int _amountOfConcurrentJobs;
		private readonly FileAssemblerServiceFactory _fileAssemblerServiceFactory;
		private readonly IFileRepository _fileRepository;

		public FileAssemblerWorker(IConfiguration configuration
			, FileAssemblerServiceFactory fileAssemblerServiceFactory
			, IFileRepository fileRepository)
		{
			_fileAssemblerServiceFactory = fileAssemblerServiceFactory;
			_fileRepository = fileRepository;
			_triggerInterval = int.Parse(configuration[Constants.FileAssemblyTriggerInterval]);
			_amountOfConcurrentJobs = int.Parse(configuration[Constants.ConcurrentFileAssemblyJobs]);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FileAssemblerWorker)}");
				while (!stoppingToken.IsCancellationRequested)
				{
					var completedFilesResult = await _fileRepository.GetCompletedFiles();
					if (completedFilesResult.WasSuccessful)
					{
						var newFiles = completedFilesResult.Data.Where(x => x.Item1.IsNew());
						var changedFiles = completedFilesResult.Data.Where(x => !x.Item1.IsNew()).OrderBy(x => x.Item1.Version);
						//could be that changes to a new file are already present. So process new first and then process changed files
						await AssembleFiles(newFiles);
						await AssembleFiles(changedFiles);

						await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
					}
				}
			});

			th.Start();

			await Task.Delay(Timeout.Infinite);
		}

		private async Task AssembleFiles(IEnumerable<(File file, EofMessage eofMesssage)> newFiles)
		{
			var taskRunner = new TaskRunner(_amountOfConcurrentJobs);
			foreach (var (file, eofMesssage) in newFiles)
				taskRunner.Add(() => _fileAssemblerServiceFactory.Get().ProcessFile(file, eofMesssage));
			
			await taskRunner.WaitTillComplete();
		}
	}
}
