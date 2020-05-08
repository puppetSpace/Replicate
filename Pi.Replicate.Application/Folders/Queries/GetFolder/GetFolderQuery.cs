using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolder
{
	public class GetFolderQuery : IRequest<Result<Folder>>
	{
		public Guid FolderId { get; set; }

	}

	public class GetFolderQueryHandler : IRequestHandler<GetFolderQuery, Result<Folder>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT Id, Name FROM dbo.Folder WHERE Id = @Id";
		private const string _selectStatementFolderRecipients = @"select re.Id,re.Name,re.Address
				from dbo.FolderRecipient fr
				inner join dbo.Recipient re on re.Id = fr.RecipientId and re.Verified = 1
				WHERE fr.FolderId = @FolderId";

		public GetFolderQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<Folder>> Handle(GetFolderQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var folder = await _database.QuerySingle<Folder>(_selectStatement, new { Id = request.FolderId });
				var recipients = await _database.Query<Recipient>(_selectStatementFolderRecipients, new { request.FolderId });
				folder.Recipients = recipients.ToList();
				return Result<Folder>.Success(folder);
			}
		}
	}
}
