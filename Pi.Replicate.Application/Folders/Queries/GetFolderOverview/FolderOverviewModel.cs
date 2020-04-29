
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pi.Replicate.Application.Common;
using System.Collections;
using System.Collections.Generic;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class FolderOverviewModel
	{
		public string FolderName { get; set; }

		public int AmountOfFilesProcessedForUpload { get; set; }

		public int AmountOfFilesProcessedForDownload { get; set; }

		public ICollection<RecipientOverviewModel> Recipients { get; set; } = new List<RecipientOverviewModel>();
	}

}