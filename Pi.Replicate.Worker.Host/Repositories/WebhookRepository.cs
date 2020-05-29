using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
    public class WebhookRepository
    {
		private readonly IDatabase _database;
		private const string _selectStatementGetWebhooks = @"
			SELECT fwh.FolderId, fwh.CallBackUrl, wht.Id, wht.[Name] Type
			FROM dbo.FolderWebhook fwh
			INNER JOIN dbo.WebhookType wht ON wht.Id = fwh.WebhookTypeId";

		public WebhookRepository(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<Webhook>>> GetWebhooks()
		{
			return await _database.Query<Webhook>(_selectStatementGetWebhooks, null);
		}
    }
}
