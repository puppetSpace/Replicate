﻿using Pi.Replicate.Application.Common;
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

		public long AmountOfChunksUploaded { get; set; }

		public long AmountOfChunksDownloaded { get; set; }

		public int AmountOfFailedFileMetadata { get; set; }

		public long AmountOfFailedFileChunks { get; set; }

		public long AmountOfFailedEofMessages { get; set; }

	}
}
