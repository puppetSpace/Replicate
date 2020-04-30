using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.Files.Commands.AddReceivedFile;
using Pi.Replicate.Application.Folders.Commands.AddReceivedFolder;

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
			var hostName = Dns.GetHostEntry(Request.HttpContext.Connection.RemoteIpAddress)?.HostName;
			var sender = string.IsNullOrWhiteSpace(hostName) ? Request.HttpContext.Connection.RemoteIpAddress.ToString() : hostName;

			var folderId = await _mediator.Send(new AddReceivedFolderCommand { Name = model.FolderName, Sender = sender });
			await _mediator.Send(new AddReceivedFileCommand { Id = model.Id, FolderId = folderId, LastModifiedDate = model.LastModifiedDate, Name = model.Name, Path = model.Path, Size = model.Size, Signature = model.Signature, Version = model.Version });
			return Ok();
		}
	}
}
