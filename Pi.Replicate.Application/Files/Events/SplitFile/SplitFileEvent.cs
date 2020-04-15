using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Events.SplitFile
{
	public class SplitFileEvent : IRequest
	{
		public File File { get; set; }
	}

	public class SplitFileEventHandler : IRequestHandler<SplitFileEvent>
	{
		private readonly FileSplitterFactory _fileSplitterFactory;
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IWorkerContext _workerContext;
		private readonly PathBuilder _pathBuilder;

		public SplitFileEventHandler(FileSplitterFactory fileSplitterFactory
		, WorkerQueueFactory workerQueueFactory
		, IWorkerContext workerContext
		, PathBuilder pathBuilder)
		{
			_fileSplitterFactory = fileSplitterFactory;
			_workerQueueFactory = workerQueueFactory;
			_workerContext = workerContext;
			_pathBuilder = pathBuilder;
		}

		public async Task<Unit> Handle(SplitFileEvent request, CancellationToken cancellationToken)
		{
			using (_workerContext)
			{
				var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var fileSplitter = _fileSplitterFactory.Get();
				int sequenceNo = 0;
				if (request.File.Status == FileStatus.New)
				{
					var fileHash = await fileSplitter.ProcessFile(request.File, async x =>
					{
						var builtChunk = FileChunk.Build(request.File.Id, sequenceNo, x, ChunkSource.FromNewFile);
						await _workerContext.FileChunkRepository.Create(builtChunk);

					});

					var delta = new Delta();
					var signature = delta.CreateSignature(_pathBuilder.BuildPath(request.File.Path));

					request.File.UpdateAfterProcessesing(sequenceNo, fileHash, signature);
					await _workerContext.FileRepository.Update(request.File);

					if (request.File.Folder.FolderOptions.DeleteAfterSent)
					{
						var path = _pathBuilder.BuildPath(request.File.Path);
						System.IO.File.Delete(path);
					}
				}
				outgoingQueue.Add(request.File);
			}
			return Unit.Value;
		}
	}
}
