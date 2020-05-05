using MediatR;
using Microsoft.EntityFrameworkCore;
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

namespace Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl
{
    public class GetFoldersToCrawlQuery : IRequest<Result<ICollection<Folder>>>
    {
        
    }

    public class GetFoldersToCrawlQueryHandler : IRequestHandler<GetFoldersToCrawlQuery, Result<ICollection<Folder>>>
    {
        private readonly IDatabase _database;
        private const string _selectStatementFolder = "SELECT Id, Name FROM dbo.Folder";
        private const string _selectStatementFolderRecipients = @"select fr.FolderId, re.Id,re.Name,re.Address
				from dbo.FolderRecipient fr
				inner join dbo.Recipient re on re.Id = fr.RecipientId";

        public GetFoldersToCrawlQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Result<ICollection<Folder>>> Handle(GetFoldersToCrawlQuery request, CancellationToken cancellationToken)
        {
			try
			{
				using (_database)
				{
					var folders = await _database.Query<Folder>(_selectStatementFolder, null);
					var folderRecipients = await _database.Query<Guid, Recipient, (Guid folderId, Recipient recipient)>(_selectStatementFolderRecipients, null, (x, y) => (x, y));
					var folderWithRecipients = folders
						.GroupJoin(folderRecipients, x => x.Id, (x) => x.folderId, (x, y) => { x.Recipients = y.Select(x => x.recipient).ToList(); return x; })
						.ToList();

					return Result<ICollection<Folder>>.Success(folders);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error happend while executing query {nameof(GetFoldersToCrawlQuery)}");
				return Result<ICollection<Folder>>.Failure();
			}
		}
    }
}
