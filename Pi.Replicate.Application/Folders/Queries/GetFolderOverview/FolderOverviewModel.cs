
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pi.Replicate.Application.Common;
using System.Collections;
using System.Collections.Generic;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class FolderOverviewModel
	{
		public string FolderName { get; set; }

		public long TotalSizeOnDisk { get; set; }

		public int LocalNewFiles{ get; set; }

		public int LocalProcessedFiles { get; set; }

		public int LocalHandledFiles { get; set; }

		public int RemoteNewFiles { get; set; }

		public int RemoteProcessedFiles { get; set; }

		public int RemoteHandledFiles { get; set; }


		public ICollection<RecipientOverviewModel> Recipients { get; set; } = new List<RecipientOverviewModel>();

		public string GetTotalSizeOnDiskDisplay() => ByteDisplayConverter.Convert(TotalSizeOnDisk);
	}

}