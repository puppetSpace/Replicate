using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Queues
{
    public class WorkerQueueContainer
    {
		public WorkerQueueContainer()
		{
			ToProcessFiles = Channel.CreateBounded<File>(new BoundedChannelOptions(50) { FullMode = BoundedChannelFullMode.Wait });
			ToSendFiles = Channel.CreateBounded<File>(new BoundedChannelOptions(50) { FullMode = BoundedChannelFullMode.Wait });
			ToSendChunks = Channel.CreateBounded<(Recipient recipient, FileChunk filechunk)>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
		}

		public Channel<File> ToProcessFiles { get; set; }

		public Channel<File> ToSendFiles { get; set; }

		public Channel<(Recipient recipient, FileChunk filechunk)> ToSendChunks { get; set; }
	}
}
