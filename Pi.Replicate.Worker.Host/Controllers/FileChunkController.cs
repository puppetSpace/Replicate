﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Services;
using System;
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

		[HttpPost("api/file/{fileId}/chunk")]
		public async Task<IActionResult> Post(Guid fileId, [FromBody] FileChunkTransmissionModel model)
		{
			var result = await _fileChunkService.AddReceivedFileChunk(fileId, model.SequenceNo, model.Value, model.Host, Request.HttpContext.Connection.RemoteIpAddress.ToString());
			return result.WasSuccessful ? NoContent() : StatusCode((int)HttpStatusCode.InternalServerError);
		}
	}
}
