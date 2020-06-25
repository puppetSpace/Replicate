using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class EofMessage
	{
		public Guid Id { get; private set; }

		public Guid FileId { get; private set; }

		public int AmountOfChunks { get; private set; }

		public DateTime CreationTime { get; private set; }

		public static EofMessage Build(Guid fileId, int amountOfChunksCreated)
		{
			return new EofMessage
			{
				Id = Guid.NewGuid(),
				FileId = fileId,
				AmountOfChunks = amountOfChunksCreated
			};
		}
	}
}
