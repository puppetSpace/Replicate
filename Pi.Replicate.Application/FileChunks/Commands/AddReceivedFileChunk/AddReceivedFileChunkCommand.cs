using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChunks.Commands.AddReceivedFileChunk
{
    public class AddReceivedFileChunkCommand : IRequest
    {
		public Guid FileId { get; set; }

		public int SequenceNo { get; set; }

		public ReadOnlyMemory<byte> Value { get; set; }
	}

	public class AddReceivedFileChunkCommandHandler : IRequestHandler<AddReceivedFileChunkCommand>
	{
		private readonly IDatabase _database;
		private const string _insertStatement = @"
	IF NOT EXISTS (SELECT 1 FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo = @SequenceNo)
		INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,[Value]) VALUES(@Id,@FileId,@SequenceNo,@Value)
";

		public AddReceivedFileChunkCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(AddReceivedFileChunkCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var fileChunk = FileChunk.Build(request.FileId, request.SequenceNo, request.Value);
				await _database.Execute(_insertStatement, new { fileChunk.Id, fileChunk.FileId, fileChunk.SequenceNo, Value = fileChunk.Value.ToArray() });
			}
			return Unit.Value;
		}
	}
}
