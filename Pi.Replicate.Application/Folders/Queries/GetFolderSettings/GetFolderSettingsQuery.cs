using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
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
		private readonly IMapper _mapper;
		private const string _selectStatementRecipients = @"
			SELECT re.Id, re.Name, re.Address
			FROM dbo.[Recipient] re
			INNER JOIN dbo.FolderRecipient frt on frt.RecipientId = re.Id and frt.FolderId = @FolderId";

		private const string _selectStatementWebhooks = @"
			SELECT fwh.Id, fwh.FolderId,fwh.CallBackUrl, wht.Id,wht.[Name], wht.[Description], wht.MessageStructure
			FROM dbo.WebHookType wht
			LEFT JOIN dbo.FolderWebHook fwh on fwh.WebHookTypeId = wht.Id and fwh.FolderId = @FolderId";

		public GetFolderSettingsQueryHandler(IDatabase database, IMapper mapper)
		{
			_database = database;
			_mapper = mapper;
		}

		public async Task<Result<FolderSettingsViewModel>> Handle(GetFolderSettingsQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var recipients = await _database.Query<Recipient>(_selectStatementRecipients, new { request.FolderId });
				var queryResultWebhooks = await _database.Query<FolderWebhook, WebhookType, FolderWebhook>(_selectStatementWebhooks, new {request.FolderId }, (f, w) => { f.WebhookType = w; return f; });
				return Result<FolderSettingsViewModel>.Success(new FolderSettingsViewModel { Recipients = recipients, FolderWebhooks = _mapper.Map<ICollection<FolderWebhookViewModel>>(queryResultWebhooks) });
			}
		}
	}
}
