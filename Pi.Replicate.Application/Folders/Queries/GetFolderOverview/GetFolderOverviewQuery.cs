using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
    public class GetFolderOverviewQuery : IRequest<FolderOverviewModel>
    {
        public Guid FolderId { get; set; }
    }

    public class GetFolderOverviewQueryHandler : IRequestHandler<GetFolderOverviewQuery, FolderOverviewModel>
    {
        //todo rework queries
        private readonly IDatabase _database;
        private const string _selectStatement = @"
			select fo.[Name] FolderName, sum(fi.Size) TotalSizeOnDisk
			, count(fi1.Id) LocalNewFiles, count(fi2.Id) LocalProcessedFiles, count(fi3.Id) LocalHandledFiles
			, count(fi4.Id) RemoteNewFiles, count(fi5.Id) RemoteProcessedFiles, count(fi6.Id) RemoteHandledFiles
			from dbo.Folder fo
			left join dbo.[File] fi on fi.FolderId = fo.Id
			left join dbo.[File] fi1 on fi1.FolderId = fo.Id and fi1.Source = 0 and fi1.[Status] = 0 --new
			left join dbo.[File] fi2 on fi2.FolderId = fo.Id and fi2.Source = 0 and fi2.[Status] = 2 --processed
			left join dbo.[File] fi3 on fi3.FolderId = fo.Id and fi3.Source = 0 and fi3.[Status] = 5 --handled
			left join dbo.[File] fi4 on fi4.FolderId = fo.Id and fi4.Source = 1 and fi4.[Status] = 3 --received remote
			left join dbo.[File] fi5 on fi5.FolderId = fo.Id and fi5.Source = 1 and fi5.[Status] = 2 --processed remote
			left join dbo.[File] fi6 on fi6.FolderId = fo.Id and fi6.Source = 1 and fi6.[Status] = 5 --handled remote
			where fo.Id = @FolderId
			group by fo.[Name]";

        private const string _recipientSelectStatement = @"
            select re.Id RecipientId, re.[Name] RecipientName, re.[Address] RecipientAddress
             , count(fil.Id) AmountOfFilesSent, sum(datalength(fcl.[Value])) - sum(datalength(fclcpl.[Value]))  TotalAmountOfBytesSent
             , count(fir.Id) AmountOfFilesReceived, sum(datalength(fcr.[Value])) TotalAmountOfBytesReceived
            from dbo.Recipient re
            inner join dbo.FolderRecipient fre on fre.RecipientId = re.Id  and fre.FolderId = @FolderId
            left join dbo.[File] fil on fil.FolderId = fre.FolderId and fil.Source = 0 and fil.Status = 5 --handled
            left join dbo.FileChunk fcl on fcl.FileId = fil.Id
			left join dbo.ChunkPackage cpl on cpl.FileChunkId = fcl.Id and cpl.RecipientId = re.Id
			left join dbo.FileChunk fclcpl on fclcpl.Id = cpl.FileChunkId
            left join dbo.[File] fir on fir.FolderId = fre.FolderId and fir.Source = 1
            left join dbo.FileChunk fcr on fcr.FileId = fir.Id
            group by re.Id, re.[Name], re.[Address]";

        public GetFolderOverviewQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<FolderOverviewModel> Handle(GetFolderOverviewQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var folderOverview = await _database.QuerySingle<FolderOverviewModel>(_selectStatement, new { request.FolderId });
                var recipients = await _database.Query<RecipientOverviewModel>(_recipientSelectStatement, new { request.FolderId });
                folderOverview.Recipients = recipients;
                return folderOverview;
            }
        }
    }
}