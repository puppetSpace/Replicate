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

namespace Pi.Replicate.Application.FolderWebhooks.Queries.GetFolderWebhooks
{
    public class GetFolderWebhooksQuery : IRequest<Result<ICollection<FolderWebhook>>>
    {
        
    }

	public class GetFolderWebhooksQueryHandler : IRequestHandler<GetFolderWebhooksQuery, Result<ICollection<FolderWebhook>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"
			SELECT fwh.Id,fwh.FolderId, fwh.CallBackUrl, wht.Id, wht.[Name], wht.[Description]
			FROM dbo.FolderWebhook fwh
			INNER JOIN dbo.WebhookType wht ON wht.Id = fwh.WebhookTypeId";

		public GetFolderWebhooksQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<FolderWebhook>>> Handle(GetFolderWebhooksQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var queryResult = await _database.Query<FolderWebhook, WebhookType, FolderWebhook>(_selectStatement, null, (f, w) =>
					{
						f.WebhookType = w;
						return f;
					});

					return Result<ICollection<FolderWebhook>>.Success(queryResult);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFolderWebhooksQuery)}'");
				return Result<ICollection<FolderWebhook>>.Failure();
			}
		}
	}
}
