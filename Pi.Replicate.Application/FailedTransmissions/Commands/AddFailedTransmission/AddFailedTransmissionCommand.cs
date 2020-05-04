﻿using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FailedTransmissions.Commands.AddFailedTransmission
{
    public class AddFailedFileTransmissionCommand : IRequest
    {
		public Guid FileId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedEofMessageTransmissionCommand : IRequest
	{
		public EofMessage EofMessage { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedFileChunkTransmissionCommand : IRequest
	{
		public FileChunk FileChunk { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class AddFailedTransmissionCommandHandler : IRequestHandler<AddFailedFileTransmissionCommand>, IRequestHandler<AddFailedEofMessageTransmissionCommand>, IRequestHandler<AddFailedFileChunkTransmissionCommand>
	{
		private readonly IDatabase _database;
		private const string _insertStatement = "INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileId,EofMessageId,FileChunkId) VALUES(@Id,@RecipientId,@FileId,@EofMessageId,@FileChunkId)";
		private const string _insertStatementEofMessage = @"IF NOT EXISTS(SELECT 1 FROM dbo.EofMessage WHERE Id = @Id)
															BEGIN
																INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id, @FileId,@AmountOfChunks)
															END";
		private const string _insertStatementFileChunk = @"IF NOT EXISTS(SELECT 1 FROM dbo.FileChunk WHERE Id = @Id)
															BEGIN
																INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,Value) VALUES(@Id,@FileId,@SequenceNo,@Value)
															END";

		public AddFailedTransmissionCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(AddFailedFileTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				string eofMessageId = null;
				string fileChunkId = null;
				await _database.Execute(_insertStatement, new { Id = Guid.NewGuid(), request.RecipientId, request.FileId, EofMessageId = eofMessageId, FileChunkId = fileChunkId });
			}

			return Unit.Value;
		}

		public async Task<Unit> Handle(AddFailedEofMessageTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				string fileId = null;
				string fileChunkId = null;
				await _database.Execute(_insertStatementEofMessage, new { request.EofMessage.Id, request.EofMessage.FileId, request.EofMessage.AmountOfChunks });
				await _database.Execute(_insertStatement, new { Id = Guid.NewGuid(), request.RecipientId, EofMessageId = request.EofMessage.Id, FileId = fileId, FileChunkId = fileChunkId });
			}

			return Unit.Value;
		}

		public async Task<Unit> Handle(AddFailedFileChunkTransmissionCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				string fileId = null;
				string eofMessageId = null;

				await _database.Execute(_insertStatementFileChunk, new { request.FileChunk.Id, request.FileChunk.FileId, request.FileChunk.SequenceNo, Value = request.FileChunk.Value.ToArray()});
				await _database.Execute(_insertStatement, new { Id = Guid.NewGuid(), request.RecipientId, FileChunkId = request.FileChunk.Id, FileId = fileId, EofMessageId = eofMessageId });
			}

			return Unit.Value;
		}
	}
}
