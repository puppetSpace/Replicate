﻿namespace Pi.Replicate.Worker.Host.Models
{
	public class FileChunkTransmissionModel
	{
		public byte[] Value { get; set; }

		public string Host { get; set; }
	}
}
