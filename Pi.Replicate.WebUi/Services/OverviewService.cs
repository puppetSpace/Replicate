using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.WebUi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Services
{
    public class OverviewService
    {
		private readonly IDatabase _database;
		private const string _selectStatement = @"
			select count(eme.FileId) AmountOfFilesProcessedForSending, count(fir.Id) AmountOfFilesProcessedForDownload
			from dbo.Folder fo
			left join dbo.[File] fil on fil.FolderId = fo.Id and fil.Source = 0
			left join dbo.EofMessage eme on eme.FileId = fil.Id
			left join dbo.[File] fir on fir.FolderId = fo.Id and fir.Source = 1 and fir.[Status] = 2";

		public OverviewService(IDatabase database)
		{
			_database = database;
		}

		public async Task<OverviewModel> GetOverview()
		{
			using (_database)
			{
				return await _database.QuerySingle<OverviewModel>(_selectStatement, null);
			}
		}
    }
}
