using Pi.Replicate.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderOverview
{
	public class RecipientOverviewModel
	{
		public Guid RecipientId { get; set; }

		public string RecipientName { get; set; }

		public string RecipientAddress { get; set; }

		public int AmountOfFilesSent { get; set; }

		public int AmountOfFilesReceived { get; set; }

		public long TotalAmountOfBytesSent { get; set; }

		public long TotalAmountOfBytesReceived { get; set; }

		public string GetTotalAmountOfBytesSentDisplay() => ByteDisplayConverter.Convert(TotalAmountOfBytesSent);
		public string GetTotalAmountOfBytesReceivedDisplay() => ByteDisplayConverter.Convert(TotalAmountOfBytesReceived);
	}
}
