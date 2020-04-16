using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.SplitFile
{
	public class SplitFileCommand : IRequest
	{
		public File File { get; set; }

		public FolderOption FolderOptions { get; set; }
	}

	public class SplitFileCommandHandler : IRequestHandler<SplitFileCommand>
	{
		private readonly FileSplitterFactory _fileSplitterFactory;
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IWorkerContext _workerContext;
		private readonly PathBuilder _pathBuilder;

		public SplitFileCommandHandler(FileSplitterFactory fileSplitterFactory
		, WorkerQueueFactory workerQueueFactory
		, IWorkerContext workerContext
		, PathBuilder pathBuilder)
		{
			_fileSplitterFactory = fileSplitterFactory;
			_workerQueueFactory = workerQueueFactory;
			_workerContext = workerContext;
			_pathBuilder = pathBuilder;
		}

		public async Task<Unit> Handle(SplitFileCommand request, CancellationToken cancellationToken)
		{
			var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
			var fileSplitter = _fileSplitterFactory.Get();
			int sequenceNo = 0;
			if (request.File.Status == FileStatus.New)
			{
				byte[] fileHash;
				using (var connection = _workerContext.BuildConnection())
				{
					await connection.OpenAsync();
					fileHash = await fileSplitter.ProcessFile(request.File, async x =>
					{
						var builtChunk = FileChunk.Build(request.File.Id, ++sequenceNo, x, ChunkSource.FromNewFile);
						Log.Verbose($"Inserting chunk {builtChunk.SequenceNo} from '{request.File.Path}' into database");
						await _workerContext.FileChunkRepository.Create(builtChunk, connection);
					});
				}

				var delta = new Delta();
				var signature = delta.CreateSignature(_pathBuilder.BuildPath(request.File.Path));

				request.File.UpdateAfterProcessesing(sequenceNo, fileHash, signature);
				await _workerContext.FileRepository.Update(request.File);

				if (request.FolderOptions.DeleteAfterSent)
				{
					var path = _pathBuilder.BuildPath(request.File.Path);
					System.IO.File.Delete(path);
				}
			}
			outgoingQueue.Add(request.File);
			Log.Information("Waiting 2 minutes to act as delay");
			await Task.Delay(TimeSpan.FromMinutes(2));
			return Unit.Value;
		}
	}
}
