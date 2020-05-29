using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared.Models;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class GetFolderOverviewQuery : IRequest<Result<FolderOverviewModel>>
	{
		public Guid FolderId { get; set; }
	}

	public class GetFolderOverviewQueryHandler : IRequestHandler<GetFolderOverviewQuery, Result<FolderOverviewModel>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"				
			select fo.[Name] FolderName, count(eme.FileId) AmountOfFilesProcessedForSending, count(fir.Id) AmountOfFilesProcessedForDownload, count(fif.Id) AmountOfFilesFailedToProcess
			from dbo.Folder fo
			left join dbo.[File] fil on fil.FolderId = fo.Id and fil.Source = 0
			left join dbo.EofMessage eme on eme.FileId = fil.Id
			left join dbo.[File] fir on fir.FolderId = fo.Id and fir.Source = 1 and fir.[Status] = 2
			left join dbo.[File] fif on fif.FolderId = fo.Id and fif.Source = 0 and fir.[Status] = 1
			where fo.Id = @FolderId
			group by fo.[Name]";

		private const string _recipientSelectStatement = @"
			select re.Id RecipientId, re.[Name] RecipientName, re.[Address] RecipientAddress, ref.AmountofFilesSent, rer.AmountOfFilesReceived
			, count(ftn.fileId) AmountOfFailedFileInfo
			, count(ftn.FileChunkId) AmountOfFailedFileChunks
			, count(ftn.EofMessageId) AmountOfFailedEofMessages
			from dbo.Recipient re
			inner join dbo.FolderRecipient fre on fre.RecipientId = re.Id and fre.FolderId = @FolderId
			left join dbo.V_AmountOfFilesSentByRecipient ref on ref.RecipientId = re.Id and ref.FolderId = fre.FolderId
			left join dbo.V_AmountOfFilesReceivedByRecipient rer on rer.RecipientId = re.Id and rer.FolderId = fre.FolderId
			left join dbo.FailedTransmission ftn on ftn.RecipientId = re.Id
			where re.Verified = 1
			group by re.Id, re.[Name], re.[Address],ref.AmountofFilesSent,rer.AmountOfFilesReceived";

		public GetFolderOverviewQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<FolderOverviewModel>> Handle(GetFolderOverviewQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var folderOverview = await _database.QuerySingle<FolderOverviewModel>(_selectStatement, new { request.FolderId });
				var recipients = await _database.Query<RecipientOverviewModel>(_recipientSelectStatement, new { request.FolderId });
				folderOverview.Recipients = recipients;
				return Result<FolderOverviewModel>.Success(folderOverview);
			}
		}
	}
}