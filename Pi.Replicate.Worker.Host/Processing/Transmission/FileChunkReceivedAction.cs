using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Processing.Transmission
{
    public class FileChunkReceivedAction
    {
		private readonly FileChunkService _fileChunkService;

		public FileChunkReceivedAction(FileChunkService fileChunkService)
		{
			_fileChunkService = fileChunkService;
		}

		public async Task<bool> Execute(Guid fileId, int sequenceNo,byte[] value,string host)
		{
			var result = await _fileChunkService.AddReceivedFileChunk(fileId, sequenceNo,value, host, DummyAdress.Create(host));
			return result.WasSuccessful;
		}
    }
}
