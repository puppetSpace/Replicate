using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Serilog;

namespace Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileTransmissions
{
	public class GetFailedFileTransmissionsForRetryQuery : IRequest<Result<ICollection<FailedFile>>>
	{

	}

	public class GetFailedFileTransmissionsForRetryQueryHandler : IRequestHandler<GetFailedFileTransmissionsForRetryQuery, Result<ICollection<FailedFile>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT fi.Id, fi.FolderId, fi.Version, fi.LastModifiedDate, fi.Name, fi.Path, fi.Size, fi.Source 
												,fo.Id, fo.Name
												,re.Id, re.Name, re.Address
					FROM dbo.[File] fi
					INNER JOIN dbo.FailedTransmission ftn on ftn.FileId = fi.id
					LEFT JOIN dbo.Folder fo on fo.Id = fi.FolderId
					LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId
					WHERE fi.Source = 0";

		public GetFailedFileTransmissionsForRetryQueryHandler(IDatabase database)
		{
			_database = database;
		}
		public async Task<Result<ICollection<FailedFile>>> Handle(GetFailedFileTransmissionsForRetryQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var result = await _database.Query<File, Folder, Recipient, FailedFile>(_selectStatement, null
					, (fi, fo, re) => new FailedFile { File = fi, Folder = fo, Recipient = re }
					);

					return Result<ICollection<FailedFile>>.Success(result);
				}
			}
			catch (System.Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFailedFileTransmissionsForRetryQuery)}'");
				return Result<ICollection<FailedFile>>.Failure();
			}
		}
	}
}