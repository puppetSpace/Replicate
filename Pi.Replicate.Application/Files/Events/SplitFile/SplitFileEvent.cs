using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
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
		private readonly IFileSplitterFactory _fileSplitterFactory;
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IWorkerContext _workerContext;
		private readonly PathBuilder _pathBuilder;

		public SplitFileEventHandler(IFileSplitterFactory fileSplitterFactory
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
			var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
			var fileSplitter = _fileSplitterFactory.Get();
			int sequenceNo = 0;
			if (request.File.Status == FileStatus.New)
			{
				//todo Save regulary to reduce memory consumption
				var result = await fileSplitter.ProcessFile(request.File, async x =>
				{
					var builtChunk = FileChunk.Build(request.File, sequenceNo, x, ChunkSource.FromNewFile);
					_workerContext.FileChunks.Add(builtChunk);
				});

				request.File.UpdateAfterProcessesing(sequenceNo, result);
				_workerContext.Files.Update(request.File);

				await _workerContext.SaveChangesAsync(cancellationToken);

				if (request.File.Folder.FolderOptions.DeleteAfterSent)
				{
					var path = _pathBuilder.BuildPath(request.File);
					System.IO.File.Delete(path);
				}
			}
			outgoingQueue.Add(request.File);

			return Unit.Value;
		}
	}
}
