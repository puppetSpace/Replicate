using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FailedTransmissions.Commands.DeleteFailedTransmission
{
	public class DeleteFailedFileTransmissionCommand : IRequest<Result>
	{
		public Guid FileId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class DeleteFailedEofMessageTransmissionCommand : IRequest<Result>
	{
		public Guid EofMessageId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class DeleteFailedFileChunkTransmissionCommand : IRequest<Result>
	{
		public Guid FileChunkId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class DeleteFailedTransmissionCommandHandler : IRequestHandler<DeleteFailedFileTransmissionCommand, Result>, IRequestHandler<DeleteFailedEofMessageTransmissionCommand, Result>, IRequestHandler<DeleteFailedFileChunkTransmissionCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _deleteFailedFileStatement = "DELETE FROM dbo.FailedTransmission WHERE FileId = @FileId and RecipientId = @RecipientId";
		private const string _deleteFailedEofMessageStatement = "DELETE FROM dbo.FailedTransmission WHERE EofMessageId = @EofMessageId and RecipientId = @RecipientId";
		private const string _deleteFailedFileChunkStatement = "DELETE FROM dbo.FailedTransmission WHERE FileChunkId = @FileChunkId and RecipientId = @RecipientId";
		private const string _deleteFileChunkStatement = "DELETE FROM dbo.FileChunk WHERE FileChunkId = @FileChunkId";

		public DeleteFailedTransmissionCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(DeleteFailedFileTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_deleteFailedFileStatement, new { request.FileId, request.RecipientId });
			return Result.Success();
		}

		public async Task<Result> Handle(DeleteFailedEofMessageTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_deleteFailedEofMessageStatement, new { request.EofMessageId, request.RecipientId });
			return Result.Success();
		}

		public async Task<Result> Handle(DeleteFailedFileChunkTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_deleteFailedFileChunkStatement, new { request.FileChunkId, request.RecipientId });
				await _database.Execute(_deleteFileChunkStatement, new { request.FileChunkId });
			}
			return Result.Success();
		}
	}
}
