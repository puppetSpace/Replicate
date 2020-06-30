using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing.Transmission;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly TransmissionActionFactory _transmissionActionFactory;

		public FileController(TransmissionActionFactory transmissionActionFactory)
		{
			_transmissionActionFactory = transmissionActionFactory;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] FileTransmissionModel model)
		{
			WorkerLog.Instance.Information($"File data received from {model.Host}");
			var isSuccessful = await _transmissionActionFactory
			.GetForFileReceived()
			.Execute(model.Id, model.FolderName, model.Name, model.Size, model.Version, model.LastModifiedDate, model.Path, model.Host);

			return isSuccessful
			? NoContent()
			: StatusCode((int)HttpStatusCode.InternalServerError);
		}
	}
}
