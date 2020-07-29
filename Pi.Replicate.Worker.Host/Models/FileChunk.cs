using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class FileChunk
	{
		private readonly byte[] _value;

		public FileChunk()
		{

		}

		public FileChunk(Guid fileId, int sequenceNo, byte[] value):base()
		{
			Id = Guid.NewGuid();
			FileId = fileId;
			SequenceNo = sequenceNo;
			_value = value;
		}

		public Guid Id { get; private set; }

		public Guid FileId { get; private set; }

		public int SequenceNo { get; private set; }

		public byte[] GetValue()
		{
			return _value;
		}
	}

	public class ReceivedFileChunk : FileChunk
	{
		public ReceivedFileChunk(Guid fileId, int sequenceNo,byte[] value,string sender, string senderAddress):base(fileId,sequenceNo,value)
		{
			Sender = sender;
			SenderAddress = senderAddress;
		}

		public string Sender { get; set; }

		public string SenderAddress { get; set; }

	}
}