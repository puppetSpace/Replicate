using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Processing.Transmission;
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
		private readonly TransmissionActionFactory _transmissionActionFactory;

		public FileChunkController(TransmissionActionFactory transmissionActionFactory)
		{
			_transmissionActionFactory = transmissionActionFactory;
		}

		[HttpPost("api/file/{fileId}/chunk/{sequenceNo}/{host}")]
		public async Task<IActionResult> Post(Guid fileId, int sequenceNo, string host)
		{
			WorkerLog.Instance.Information($"Filechunk received from {host}");
			using (var ms = new MemoryStream())
			{
				await Request.Body.CopyToAsync(ms);
				var isSuccessful = await _transmissionActionFactory
					.GetForFileChunkReceived()
					.Execute(fileId, sequenceNo, ms.ToArray(), host);
				return isSuccessful 
					? NoContent() 
					: StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}
	}
}
