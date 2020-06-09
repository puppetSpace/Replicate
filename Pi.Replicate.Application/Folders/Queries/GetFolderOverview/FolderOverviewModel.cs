using System.Collections.Generic;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class FolderOverviewModel
	{
		public string FolderName { get; set; }

		public int AmountOfFilesProcessedForSending { get; set; }

		public int AmountOfFilesProcessedForDownload { get; set; }

		public int AmountOfFilesFailedToProcess { get; set; }

		public int AmountOfConflicts { get; set; }

		public ICollection<RecipientOverviewModel> Recipients { get; set; } = new List<RecipientOverviewModel>();
	}

}