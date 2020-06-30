using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing.Transmission;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	public class EofMessageController : ControllerBase
	{
		private readonly TransmissionActionFactory _transmissionActionFactory;

		public EofMessageController(TransmissionActionFactory transmissionActionFactory)
		{
			_transmissionActionFactory = transmissionActionFactory;
		}

		[HttpPost("api/file/{fileId}/eot")]
		public async Task<IActionResult> Post(Guid fileId, [FromBody] EofMessageTransmissionModel model)
		{
			WorkerLog.Instance.Information($"Eof message received from {Request.HttpContext.Connection.RemoteIpAddress}");
			var isSuccessfull = await _transmissionActionFactory
				.GetForEofMessageReceived()
				.Execute(fileId, model.AmountOfChunks);
			return isSuccessfull 
				? NoContent() 
				: StatusCode((int)HttpStatusCode.InternalServerError);
		}

	}
}
