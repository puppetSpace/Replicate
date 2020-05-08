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

		public Guid FileId { get; set; }

		public int SequenceNo { get; set; }

		public ReadOnlyMemory<byte> Value { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedTransmissionCommandHandler : IRequestHandler<AddFailedFileTransmissionCommand, Result>, IRequestHandler<AddFailedEofMessageTransmissionCommand, Result>, IRequestHandler<AddFailedFileChunkTransmissionCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _insertStatementForFile = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileId = @FileId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileId) VALUES(@Id,@RecipientId,@FileId)";

		private const string _insertStatementForEofMessage = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE EofMessageId = @EofMessageId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,EofMessageId) VALUES(@Id,@RecipientId,@EofMessageId)";

		private const string _insertStatementForFileChunk = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileChunkId = @FileChunkId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileChunkId) VALUES(@Id,@RecipientId,@FileChunkId)";

		private const string _insertStatementFileChunk = @"IF NOT EXISTS(SELECT 1 FROM dbo.FileChunk WHERE Id = @Id)
															BEGIN
																INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,Value) VALUES(@Id,@FileId,@SequenceNo,@Value)
															END";


		public AddFailedTransmissionCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(AddFailedFileTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_insertStatementForFile, new { Id = Guid.NewGuid(), request.RecipientId, request.FileId });
			return Result.Success();
		}

		public async Task<Result> Handle(AddFailedEofMessageTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_insertStatementForEofMessage, new { Id = Guid.NewGuid(), request.RecipientId, EofMessageId = request.EofMessageId });

			return Result.Success();
		}

		public async Task<Result> Handle(AddFailedFileChunkTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_insertStatementFileChunk, new { Id = Guid.NewGuid(), request.RecipientId, request.FileChunkId });
				//the filechunk must be save so it can be retrieved again for resend
				await _database.Execute(_insertStatementFileChunk, new { Id = request.FileChunkId, request.FileId, request.SequenceNo, Value = request.Value.ToArray() });

			}
			return Result.Success();
		}
	}
}
