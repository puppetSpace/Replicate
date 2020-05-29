using Microsoft.AspNetCore.Mvc;
using Observr;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly FolderRepository _folderRepository;
		private readonly RecipientRepository _recipientRepository;
		private readonly IFileRepository _fileRespository;
		private readonly PathBuilder _pathBuilder;
		private readonly IBroker _broker;

		public FileController(FolderRepository folderRepository
			, RecipientRepository recipientRepository
			, IFileRepository fileRespository
			, PathBuilder pathBuilder
			, IBroker broker)
		{
			_folderRepository = folderRepository;
			_recipientRepository = recipientRepository;
			_fileRespository = fileRespository;
			_pathBuilder = pathBuilder;
			_broker = broker;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] FileTransmissionModel model)
		{
			var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
			Log.Information($"File data received from {ipAddress}");
			var folderCreation = await AddFolder(model.FolderName);
			if (folderCreation.WasSuccessful)
			{
				var fileResult = await _fileRespository.AddNewFile(new File { Id = model.Id, FolderId = folderCreation.Data, Name = model.Name, Size = model.Size, Version = model.Version, LastModifiedDate = model.LastModifiedDate, Path = model.Path, Source = FileSource.Remote });
				var recipientResult = await _recipientRepository.AddRecipientToFolder(model.Host, $"https://{ipAddress}:{Request.HttpContext.Connection.RemotePort}", folderCreation.Data);
				return fileResult.WasSuccessful && recipientResult.WasSuccessful ? NoContent() : StatusCode((int)HttpStatusCode.InternalServerError);
			}
			else
			{
				return StatusCode((int)HttpStatusCode.InternalServerError);

			}
		}

		private async Task<Result<Guid>> AddFolder(string folderName)
		{
			var folderAddResult = await _folderRepository.AddFolder(folderName);
			if (folderAddResult.WasSuccessful)
			{
				var folderPath = _pathBuilder.BuildPath(folderName);
				if (!System.IO.Directory.Exists(folderPath))
					System.IO.Directory.CreateDirectory(folderPath);
				_broker.Publish(new Folder { Id = folderAddResult.Data, Name = folderName }).Forget();

				return folderAddResult;
			}
			else
			{
				return Result<Guid>.Failure();
			}
		}
	}
}
