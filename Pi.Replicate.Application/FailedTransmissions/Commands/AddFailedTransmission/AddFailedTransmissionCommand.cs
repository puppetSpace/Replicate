using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FailedTransmissions.Commands.AddFailedTransmission
{
	public class AddFailedFileTransmissionCommand : IRequest<Result>
	{
		public Guid FileId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedEofMessageTransmissionCommand : IRequest<Result>
	{
		public Guid EofMessageId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedFileChunkTransmissionCommand : IRequest<Result>
	{
		public Guid FileChunkId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedTransmissionCommandHandler : IRequestHandler<AddFailedFileTransmissionCommand, Result>, IRequestHandler<AddFailedEofMessageTransmissionCommand, Result>, IRequestHandler<AddFailedFileChunkTransmissionCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _insertStatementFile = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileId = @FileId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileId) VALUES(@Id,@RecipientId,@FileId)";

		private const string _insertStatementEofMessage = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE EofMessageId = @EofMessageId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,EofMessageId) VALUES(@Id,@RecipientId,@EofMessageId)";

		private const string _insertStatementFileChunk = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileChunkId = @FileChunkId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileChunkId) VALUES(@Id,@RecipientId,@FileChunkId)";


		public AddFailedTransmissionCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(AddFailedFileTransmissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					await _database.Execute(_insertStatementFile, new { Id = Guid.NewGuid(), request.RecipientId, request.FileId });
				return Result.Success();
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing command '{nameof(AddFailedFileTransmissionCommand)}'");
				return Result.Failure();
			}
		}

		public async Task<Result> Handle(AddFailedEofMessageTransmissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					await _database.Execute(_insertStatementEofMessage, new { Id = Guid.NewGuid(), request.RecipientId, EofMessageId = request.EofMessageId });

				return Result.Success();
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing command '{nameof(AddFailedEofMessageTransmissionCommand)}'");
				return Result.Failure();
			}
		}

		public async Task<Result> Handle(AddFailedFileChunkTransmissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					await _database.Execute(_insertStatementFileChunk, new { Id = Guid.NewGuid(), request.RecipientId, FileChunkId = request.FileChunkId });
				return Result.Success();
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing command '{nameof(AddFailedFileChunkTransmissionCommand)}'");
				return Result.Failure();
			}
		}
	}
}
