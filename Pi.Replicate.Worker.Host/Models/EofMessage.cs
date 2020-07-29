using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class EofMessage
	{

		public EofMessage()
		{

		}

		public EofMessage(Guid fileId, int amountOfChunks) : this()
		{
			Id = Guid.NewGuid();
			FileId = fileId;
			AmountOfChunks = amountOfChunks;
			CreationTime = DateTime.UtcNow;
		}

		public Guid Id { get; private set; }

		public Guid FileId { get; private set; }

		public int AmountOfChunks { get; private set; }

		public DateTime CreationTime { get; private set; }
	}
}
