using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class FileChunkController : ControllerBase
	{
		private readonly IFileChunkRepository _fileChunkRepository;

		public FileChunkController(IFileChunkRepository fileChunkRepository)
		{
			_fileChunkRepository = fileChunkRepository;
		}

		[HttpPost("api/file/{fileId}/chunk/{sequenceNo}/{host}")]
		public async Task<IActionResult> Post(Guid fileId, int sequenceNo, string host)
		{
			WorkerLog.Instance.Information($"Filechunk received from {host}");
			using (var ms = new MemoryStream())
			{
				await Request.Body.CopyToAsync(ms);
				var result = await _fileChunkRepository.AddReceivedFileChunk(fileId, sequenceNo, ms.ToArray(), host, DummyAdress.Create(host));
				return result.WasSuccessful
					? NoContent() 
					: StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}
	}
}
