using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class FileChunkController : ControllerBase
	{
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public FileChunkController(RecyclableMemoryStreamManager recyclableMemoryStreamManager, WorkerQueueContainer workerQueueContainer)
		{
			_recyclableMemoryStreamManager = recyclableMemoryStreamManager;
			_workerQueueContainer = workerQueueContainer;
		}

		//todo test
		[HttpPost("api/file/{fileId}/chunk/{sequenceNo}/{host}")]
		public async Task<IActionResult> Post(Guid fileId, int sequenceNo, string host)
		{
			WorkerLog.Instance.Information($"Filechunk received from {host}");
			using (var ms = _recyclableMemoryStreamManager.GetStream())
			{
				var outgoingQueue = _workerQueueContainer.ReceivedChunks.Writer;
				await Request.Body.CopyToAsync(ms);
				if (ms.TryGetBuffer(out var buffer) && await outgoingQueue.WaitToWriteAsync())
				{
					await outgoingQueue.WriteAsync(new ReceivedFileChunk(fileId, sequenceNo, buffer.Array, host, DummyAdress.Create(host)));
					return NoContent();
				}
				else
				{
					return StatusCode(500);
				}
			}
		}
	}
}
