using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddReceivedFile
{
    public class AddReceivedFileCommand : IRequest
    {
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Name { get; set; }

		public long Size { get; set; }

		public int Version { get; set; }

		public byte[] Signature { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string Path { get; set; }
	}

	public class AddReceivedFileCommandHandler : IRequestHandler<AddReceivedFileCommand>
	{
		private readonly IDatabase _database;
		private const string _fileInsertStatement = "INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)";
		
		public AddReceivedFileCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(AddReceivedFileCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_fileInsertStatement, new { request.Id, request.FolderId, request.Name, request.Size, request.Version, request.LastModifiedDate, request.Path, request.Signature, Source = FileSource.Remote });

			return Unit.Value;
		}
	}
}
