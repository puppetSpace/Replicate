using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class FileChunkController : ControllerBase
	{
		private readonly IFileChunkRepository _fileChunkRepository;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

		public FileChunkController(IFileChunkRepository fileChunkRepository, RecyclableMemoryStreamManager recyclableMemoryStreamManager)
		{
			_fileChunkRepository = fileChunkRepository;
			_recyclableMemoryStreamManager = recyclableMemoryStreamManager;
		}

		//todo test
		[HttpPost("api/file/{fileId}/chunk/{sequenceNo}/{host}")]
		public async Task<IActionResult> Post(Guid fileId, int sequenceNo, string host)
		{
			WorkerLog.Instance.Information($"Filechunk received from {host}");
			using (var ms = _recyclableMemoryStreamManager.GetStream())
			{
				await Request.Body.CopyToAsync(ms);
				if (ms.TryGetBuffer(out var buffer))
				{
					var result = await _fileChunkRepository.AddReceivedFileChunk(fileId, sequenceNo, buffer.Array, host, DummyAdress.Create(host));
					return result.WasSuccessful
						? NoContent()
						: StatusCode(500);
				}
				else
				{
					return StatusCode(500);
				}
			}
		}
	}
}
