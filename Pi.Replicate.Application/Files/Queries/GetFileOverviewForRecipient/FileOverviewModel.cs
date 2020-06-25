using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFileOverviewForRecipient
{
    public class FileOverviewModel
    {
		public FileOverviewModel()
		{
			Versions = new List<FileOverviewModel>();
		}
		public string Name { get; set; }

		public int? Version { get; set; }

		public long? Size { get; set; }

		public DateTime? LastModifiedDate { get; set; }

		public string Path { get; set; }

		public double PercentageSentReceived { get; set; }

		public DateTime? LastSent { get; set; }

		public FileSource Source { get; set; }

		public bool MetadataPresent { get; set; }

		public bool EofMessagePresent { get; set; }

		public ICollection<FileOverviewModel> Versions { get; set; }

		public string GetSizeDisplayModel() => ByteDisplayConverter.Convert(Size ?? 0);
	}
}
