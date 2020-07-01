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
		private readonly IDeltaService _deltaService;
		private readonly IFileRepository _fileRespository;
		private readonly PathBuilder _pathBuilder;
		private readonly FolderRepository _folderRepository;
		private readonly RecipientRepository _recipientRepository;

		public FileService(IDeltaService deltaService
			, IFileRepository fileRespository
			, PathBuilder pathBuilder
			, FolderRepository folderRepository
			, RecipientRepository recipientRepository)
		{
			_deltaService = deltaService;
			_fileRespository = fileRespository;
			_pathBuilder = pathBuilder;
			_folderRepository = folderRepository;
			_recipientRepository = recipientRepository;
		}

		public async Task<File> CreateNewFile(Guid folderId, System.IO.FileInfo newFile)
		{
			var signature = _deltaService.CreateSignature(newFile.FullName);
			var file = File.Build(newFile, folderId, _pathBuilder.BasePath);

			var result = await _fileRespository.AddNewFile(file, signature.ToArray());
			return result.WasSuccessful ? file : null;
		}

		public async Task<File> CreateUpdateFile(Guid folderId, System.IO.FileInfo changedFile)
		{
			var signature = _deltaService.CreateSignature(changedFile.FullName);
			var relativePath = changedFile.FullName.Replace(_pathBuilder.BasePath + "\\", "");
			var queryResult = await _fileRespository.GetLastVersionOfFile(folderId, relativePath);
			if (queryResult.WasSuccessful && queryResult.Data is object)
			{
				queryResult.Data.Update(changedFile);
				await _fileRespository.AddNewFile(queryResult.Data, signature.ToArray());
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
				{
					Id = fileId,
					FolderId = folderCreation.Data,
					Name = name,
					Size = size,
					Version = version,
					LastModifiedDate = lastModifiedDate,
					Path = path,
					Source = FileSource.Remote
				});
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
				var folderPath = _pathBuilder.BuildPath(folderName);
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
