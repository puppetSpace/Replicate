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
            , count(fil.Id) AmountOfFilesSent, sum(datalength(fcl.[Value])) - sum(datalength(fclcpl.[Value])) TotalAmountOfBytesSent
            , count(fir.Id) AmountOfFilesReceived, sum(datalength(fcr.[Value])) TotalAmountOfBytesReceived
            from dbo.Folder fo
            left join dbo.[File] fi on fi.FolderId = fo.Id
            left join dbo.[File] fil on fil.FolderId = fo.Id and fil.Source = 0 and fil.Status = 5 --handled
            left join dbo.FileChunk fcl on fcl.FileId = fil.Id
			left join dbo.ChunkPackage cpl on cpl.FileChunkId = fcl.Id
			left join dbo.FileChunk fclcpl on fclcpl.Id = cpl.FileChunkId
            left join dbo.[File] fir on fir.FolderId = fo.Id and fir.Source = 1
            left join dbo.FileChunk fcr on fcr.FileId = fir.Id
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