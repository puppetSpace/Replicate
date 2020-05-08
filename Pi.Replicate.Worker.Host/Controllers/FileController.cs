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
using Serilog;

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
			var folderIdResult = await _mediator.Send(new AddReceivedFolderCommand { Name = model.FolderName, Sender = model.Host, SenderAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString() });
			if (folderIdResult.WasSuccessful)
			{
				await _mediator.Send(new AddReceivedFileCommand { Id = model.Id, FolderId = folderIdResult.Data, LastModifiedDate = model.LastModifiedDate, Name = model.Name, Path = model.Path, Size = model.Size, Signature = model.Signature, Version = model.Version });
				return Ok();
			}
			return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
		}
	}
}
