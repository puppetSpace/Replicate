
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pi.Replicate.Application.Common;
using System.Collections;
using System.Collections.Generic;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class FolderOverviewModel
	{
		public string FolderName { get; set; }

		public int AmountOfFilesSent{ get; set; }

		public int AmountOfFilesReceived { get; set; }

		public long TotalAmountOfBytesSent { get; set; }

		public long TotalAmountOfBytesReceived { get; set; }

		public long TotalSizeOnDisk { get; set; }

		public ICollection<RecipientOverviewModel> Recipients { get; set; } = new List<RecipientOverviewModel>();

		public string GetTotalAmountOfBytesSentDisplay() => ByteDisplayConverter.Convert(TotalAmountOfBytesSent);
		public string GetTotalAmountOfBytesReceivedDisplay() => ByteDisplayConverter.Convert(TotalAmountOfBytesReceived);

		public string GetTotalSizeOnDiskDisplay() => ByteDisplayConverter.Convert(TotalSizeOnDisk);
	}

}