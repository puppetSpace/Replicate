using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl
{
    public class GetFoldersToCrawlQuery : IRequest<ICollection<Folder>>
    {
        
    }

    public class GetFoldersToCrawlQueryHandler : IRequestHandler<GetFoldersToCrawlQuery, ICollection<Folder>>
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

        public async Task<ICollection<Folder>> Handle(GetFoldersToCrawlQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var folders = await _database.Query<Folder>(_selectStatementFolder, null);
                var folderRecipients = await _database.Query<Guid, Recipient, (Guid folderId, Recipient recipient)>(_selectStatementFolderRecipients,null, (x, y) => (x, y));

                return folders
                    .GroupJoin(folderRecipients, x => x.Id, (x) => x.folderId, (x, y) => { x.Recipients = y.Select(x => x.recipient).ToList(); return x; })
                    .ToList();
            }
        }
    }
}
