using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Serilog;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder
{
	public class GetRecipientsForFolderQuery : IRequest<Result<ICollection<Recipient>>>
	{
		public Guid FolderId { get; set; }
	}

	public class GetRecipientsForFolderQueryHandler : IRequestHandler<GetRecipientsForFolderQuery, Result<ICollection<Recipient>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"
			SELECT re.Id,re.Name,re.Address 
			FROM dbo.Recipient re
			INNER JOIN dbo.FolderRecipient fr on fr.RecipientId = re.Id and fr.FolderId = @FolderId
			WHERE re.Verified = 1";

		public GetRecipientsForFolderQueryHandler(IDatabase database)
		{
			_database = database;
		}
		public async Task<Result<ICollection<Recipient>>> Handle(GetRecipientsForFolderQuery request, CancellationToken cancellationToken)
		{
			using (_database)
				return Result<ICollection<Recipient>>.Success(await _database.Query<Recipient>(_selectStatement, new { request.FolderId }));
		}
	}
}