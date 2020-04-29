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
					
		with latestFile_cte(Id, FolderId,[Version], LastModifiedDate, [Name],[Path], [Signature],Size,Source)
		as
		(SELECT TOP 1 Id, FolderId,[Version], LastModifiedDate, [Name],[Path], [Signature],Size,Source 
		FROM dbo.[File] 
		WHERE FolderId = @FolderId and [Path] = @Path 
		ORDER BY [Version] desc)
		select fo.[Name] FolderName, count(eme.FileId) AmountOfFilesProcessedForUpload, count(emer.FileId) AmountOfFilesProcessedForDownload
			from dbo.Folder fo
			left join latestFile_cte fi on fi.FolderId = fo.Id and fi.Source = 0
			left join dbo.EofMessage eme on eme.FileId = fi.Id
			left join latestFile_cte fir on fir.FolderId = fo.Id and fi.Source = 1
			left join dbo.EofMessage emer on emer.FileId = fir.Id
			where fo.Id = @FolderId
			group by fo.[Name]";

        private const string _recipientSelectStatement = @"
		select re.Id RecipientId, re.[Name] RecipientName, re.[Address] RecipientAddress
		, count(trt.FileChunkSequenceNo) AmountOfChunksUploaded
		, count(trtr.FileChunkSequenceNo) AmountOfChunksDownloaded
		, count(ftn.fileId) AmountOfFailedFileMetadata
		, count(ftn.FileChunkId) AmountOfFailedFileChunks
		from dbo.Recipient re
		inner join dbo.FolderRecipient fre on fre.RecipientId = re.Id  ---and fre.FolderId = @FolderId
		left join dbo.[File] fi on fi.FolderId = fre.FolderId and fi.Source = 0
		left join dbo.TransmissionResult trt on trt.FileId = fi.Id and trt.RecipientId = re.Id
		left join dbo.[File] fir on fir.FolderId = fre.FolderId and fir.Source = 1
		left join dbo.TransmissionResult trtr on trtr.FileId = fir.Id and trtr.RecipientId = re.Id
		left join dbo.FailedTransmission ftn on ftn.RecipientId = re.Id
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