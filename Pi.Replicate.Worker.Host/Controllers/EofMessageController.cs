using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.EofMessages.Commands.AddReceivedEofMessage;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class EofMessageController : ControllerBase
	{
		private readonly IMediator _mediator;

		public EofMessageController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("api/file/{fileId}/eot")]
		public async Task<IActionResult> Post([FromQuery] Guid fileId, [FromBody] EofMessageTransmissionModel model)
		{
			Log.Information($"Eof message received from {Request.HttpContext.Connection.RemoteIpAddress}");
			await _mediator.Send(new AddReceivedEofMessageCommand { FileId = fileId, AmountOfChunks = model.AmountOfChunks });
			return Ok();
		}

	}
}
