using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Services;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly FileService _fileService;

		public FileController(FileService fileService)
		{
			_fileService = fileService;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] FileTransmissionModel model)
		{
			WorkerLog.Instance.Information($"File data received from {model.Host}");
			var result = await _fileService.AddReceivedFile(model.Id, model.FolderName, model.Name, model.Size, model.Version, model.LastModifiedDate, model.Path, model.Host);

			return result.WasSuccessful
			? NoContent()
			: StatusCode((int)HttpStatusCode.InternalServerError);
		}
	}
}
