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

namespace Pi.Replicate.Application.Folders.Queries.GetFolderSettings
{
    public class GetFolderSettingsQuery : IRequest<Result<FolderSettingsViewModel>>
    {
		public Guid FolderId { get; set; }
	}

	public class GetFolderSettingsQueryHandler : IRequestHandler<GetFolderSettingsQuery, Result<FolderSettingsViewModel>>
	{
		private readonly IDatabase _database;
		private const string _selectStatementRecipients = @"
			SELECT re.Id, re.Name, re.Address
			FROM dbo.[Recipient] re
			INNER JOIN dbo.FolderRecipient frt on frt.RecipientId = re.Id and frt.FolderId = @FolderId";

		public GetFolderSettingsQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<FolderSettingsViewModel>> Handle(GetFolderSettingsQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var recipients = await _database.Query<Recipient>(_selectStatementRecipients, new { request.FolderId });
					return Result<FolderSettingsViewModel>.Success(new FolderSettingsViewModel { Recipients = recipients });
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFolderSettingsQuery)}'");
				return Result<FolderSettingsViewModel>.Failure();
			}
		}
	}
}
