using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.FileChunks.Commands.AddReceivedFileChunk;
using Pi.Replicate.Domain;
using Pi.Replicate.TransmissionResults.Commands.AddTransmissionResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
    public class FileChunkController : ControllerBase
    {
		private readonly IMediator _mediator;

		public FileChunkController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("api/file/{fileId}/chunk")]
		public async Task<IActionResult> Post(Guid fileId, [FromBody] FileChunkTransmissionModel model)
		{
			await _mediator.Send(new AddReceivedFileChunkCommand { FileId = fileId, SequenceNo = model.SequenceNo, Value = model.Value, Sender = model.Host, SenderAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString() });
			return Ok();
		}
    }
}
