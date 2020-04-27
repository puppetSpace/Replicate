using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Queries.GetFailedTransmissions
{
	public class GetFailedFileTransmissionsForRetryCommand : IRequest<ICollection<FailedFile>>
	{

	}

	public class GetFailedFileTransmissionsForRetryCommandHandler : IRequestHandler<GetFailedFileTransmissionsForRetryCommand, ICollection<FailedFile>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT fi.Id, fi.FolderId, fi.Version, fi.LastModifiedDate, fi.Name, fi.Path, fi.Signature, fi.Size, fi.Source 
												,fo.Id, fo.Name
												,re.Id, re.Name, re.Address
					FROM dbo.[File] fi
					INNER JOIN dbo.FailedTransmission ftn on ftn.FileId = fi.id
					LEFT JOIN dbo.Folder fo on fo.Id = fi.FolderId
					LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId
					WHERE fi.Source = 0";

		private const string _deleteStatement = "DELETE FROM dbo.FailedTransmission WHERE FileId is not null";

		public GetFailedFileTransmissionsForRetryCommandHandler(IDatabase database)
		{
			_database = database;
		}
		public async Task<ICollection<FailedFile>> Handle(GetFailedFileTransmissionsForRetryCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var result = await _database.Query< File, Folder, Recipient, FailedFile>(_selectStatement, null
				, (fi, fo, re) => new FailedFile { File = fi, Folder = fo, Recipient = re }
				);

				await _database.Execute(_deleteStatement, null);
				return result;
			}
		}
	}
}