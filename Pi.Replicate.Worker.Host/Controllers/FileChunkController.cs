using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Models;
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
		private readonly FileChunkService _fileChunkService;

		public FileChunkController(FileChunkService fileChunkService)
		{
			_fileChunkService = fileChunkService;
		}

		[HttpPost("api/file/{fileId}/chunk/{sequenceNo}/{host}")]
		public async Task<IActionResult> Post(Guid fileId, int sequenceNo, string host)
		{
			//todo create function to form address
			using (var ms = new MemoryStream())
			{
				await Request.Body.CopyToAsync(ms);
				var address = $"https://{Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}:{Request.HttpContext.Connection.RemotePort}";
				var result = await _fileChunkService.AddReceivedFileChunk(fileId, sequenceNo, ms.ToArray(), host, address);
				return result.WasSuccessful ? NoContent() : StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}
	}
}
