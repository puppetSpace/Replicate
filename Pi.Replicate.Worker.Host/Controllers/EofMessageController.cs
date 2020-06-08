using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class EofMessageController : ControllerBase
	{
		private readonly IEofMessageRepository _eofMessageRepository;

		public EofMessageController(IEofMessageRepository eofMessageRepository)
		{
			_eofMessageRepository = eofMessageRepository;
		}

		[HttpPost("api/file/{fileId}/eot")]
		public async Task<IActionResult> Post(Guid fileId, [FromBody] EofMessageTransmissionModel model)
		{
			WorkerLog.Instance.Information($"Eof message received from {Request.HttpContext.Connection.RemoteIpAddress}");
			var eofMessage = EofMessage.Build(fileId, model.AmountOfChunks);
			var result = await _eofMessageRepository.AddReceivedEofMessage(eofMessage);
			return result.WasSuccessful ? NoContent() : StatusCode((int)HttpStatusCode.InternalServerError);
		}

	}
}
