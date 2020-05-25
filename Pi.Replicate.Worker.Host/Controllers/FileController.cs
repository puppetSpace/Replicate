using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.Files.Commands.AddReceivedFile;
using Serilog;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly IMediator _mediator;

		public FileController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] FileTransmissionModel model)
		{
			Log.Information($"File data received from {Request.HttpContext.Connection.RemoteIpAddress}");
			await _mediator.Send(new AddReceivedFileCommand { Id = model.Id, FolderName = model.FolderName, LastModifiedDate = model.LastModifiedDate, Name = model.Name, Path = model.Path, Size = model.Size, Version = model.Version, Sender = model.Host, SenderAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString() });
			return Ok();
		}
	}
}
