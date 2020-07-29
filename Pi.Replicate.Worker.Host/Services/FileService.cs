using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class FileService
	{
		private readonly IFileRepository _fileRespository;
		private readonly FolderRepository _folderRepository;
		private readonly RecipientRepository _recipientRepository;

		public FileService(IFileRepository fileRespository
			, FolderRepository folderRepository
			, RecipientRepository recipientRepository)
		{
			_fileRespository = fileRespository;
			_folderRepository = folderRepository;
			_recipientRepository = recipientRepository;
		}

		public async Task<File> CreateNewFile(Guid folderId, System.IO.FileInfo newFile)
		{
			var file = new File(newFile, folderId, PathBuilder.BasePath);
			var signature = file.CreateSignature();

			var result = await _fileRespository.AddNewFile(file, signature);
			return result.WasSuccessful ? file : null;
		}

		public async Task<File> CreateUpdateFile(Guid folderId, System.IO.FileInfo changedFile)
		{
			var relativePath = changedFile.FullName.Replace(PathBuilder.BasePath + "\\", "");
			var queryResult = await _fileRespository.GetLastVersionOfFile(folderId, relativePath);
			if (queryResult.WasSuccessful && queryResult.Data is File file)
			{
				file.Update(changedFile);
				var signature = file.CreateSignature();
				await _fileRespository.AddNewFile(queryResult.Data, signature);
				return queryResult.Data;
			}

			return null;
		}

		public async Task<Result> AddReceivedFile(Guid fileId, string folderName, string name, long size, int version, DateTime lastModifiedDate, string path, string host)
		{
			var folderCreation = await AddFolder(folderName);
			if (folderCreation.WasSuccessful)
			{
				var fileResult = await _fileRespository.AddNewFile(new File
				(
					fileId,
					folderCreation.Data,
					name,
					path,
					lastModifiedDate,
					size,
					FileSource.Remote,
					version
				));
				var recipientResult = await _recipientRepository.AddRecipientToFolder(host, DummyAdress.Create(host), folderCreation.Data);

				return fileResult.WasSuccessful && recipientResult.WasSuccessful
					? Result.Success()
					: Result.Failure();
			}

			return Result.Failure();
		}

		private async Task<Result<Guid>> AddFolder(string folderName)
		{
			var folderAddResult = await _folderRepository.AddFolder(folderName);
			if (folderAddResult.WasSuccessful)
			{
				var folderPath = PathBuilder.BuildPath(folderName);
				if (!System.IO.Directory.Exists(folderPath))
					System.IO.Directory.CreateDirectory(folderPath);

				return folderAddResult;
			}
			else
			{
				return Result<Guid>.Failure();
			}
		}

	}
}
