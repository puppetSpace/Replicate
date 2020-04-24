using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class EofMessage
    {
		public Guid Id { get; private set; }

		public Guid FileId { get; private set; }

		public File File { get; private set; }

		public int AmountOfChunks { get; private set; }

		public static EofMessage Build(File file, int amountOfChunksCreated)
		{
			return new EofMessage
			{
				Id = Guid.NewGuid(),
				File = file,
				FileId = file.Id,
				AmountOfChunks = amountOfChunksCreated
			};
		}
	}
}
