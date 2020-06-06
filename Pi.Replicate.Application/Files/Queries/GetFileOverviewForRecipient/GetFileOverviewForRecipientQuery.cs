using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFileOverviewForRecipient
{
    public class GetFileOverviewForRecipientQuery : IRequest<Result<ICollection<FileOverviewModel>>>
    {
		public Guid FolderId { get; set; }

		public Guid RecipientId { get; set; }
	}

	public class GetFileOverviewForRecipientQueryHandler : IRequestHandler<GetFileOverviewForRecipientQuery, Result<ICollection<FileOverviewModel>>>
	{
		private readonly IDatabase _database;
		private const string _selectQuery = @"
			with file_cte(Id,[Name],[Version],Size,LastModifiedDate,[Path],Source,rnk) as
			(select  Id,[Name], [Version], Size,LastModifiedDate, [Path], Source
			, ROW_NUMBER() over(partition by [Name] order by [Version] DESC)  rnk
			from dbo.[File]
			where [Status] in (0,2) and FolderId = @FolderId
			),
			transmission_cte(fileId,chunkCount, LastSent) as
			(select FileId,count(FileChunkSequenceNo), max(CreationTime)
			from dbo.TransmissionResult
			where RecipientId = @RecipientId
			group by FileId)
			select fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],tr.LastSent, fi.Source, (convert(decimal,tr.chunkCount) / em.AmountOfChunks) * 100 PercentageSentReceived
			, iif(em.Id is null,0,1) EofMessagePresent
			, iif(em.Id is not null and fi.Id not in (select fileid from dbo.failedTransmission),1,0) MetadataPresent
			, fis.[Name],fis.[Version],fis.Size,fis.LastModifiedDate,fis.[Path], trs.LastSent,fis.Source, (convert(decimal,trs.chunkCount) / ems.AmountOfChunks) * 100 PercentageSentReceived
			, iif(ems.Id is null,0,1) EofMessagePresent
			, iif(ems.Id is not null and fi.Id not in (select fileid from dbo.failedTransmission),1,0) MetadataPresent
			from file_cte fi
			left join dbo.[File] fis on fis.[Path] = fi.[Path] and fis.[Version] <> fi.[Version]
			left join dbo.EofMessage em on em.FileId = fi.Id
			left join dbo.EofMessage ems on ems.FileId = fis.Id
			left join transmission_cte tr on tr.fileId = fi.Id
			left join transmission_cte trs on trs.fileId = fis.Id
			where rnk = 1";

		public GetFileOverviewForRecipientQueryHandler(IDatabase database)
		{
			_database = database;
		}


		public async Task<Result<ICollection<FileOverviewModel>>> Handle(GetFileOverviewForRecipientQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var fileDictionary = new Dictionary<string, FileOverviewModel>();
				var queryResult = await _database.Query<FileOverviewModel, FileOverviewModel, FileOverviewModel>(_selectQuery, new { request.FolderId, request.RecipientId },
					(fom1, fom2) =>
					{
						if(!fileDictionary.TryGetValue(fom1.Name,out var foundFom))
						{
							foundFom = fom1;
							foundFom.Versions = new List<FileOverviewModel>();
							fileDictionary.Add(foundFom.Name, foundFom);
						}

						if(fom2 is object)
							foundFom.Versions.Add(fom2);
						return fom1;
					},splitOn: "Name");

				return Result<ICollection<FileOverviewModel>>.Success(queryResult);
			}
		}
	}
}
