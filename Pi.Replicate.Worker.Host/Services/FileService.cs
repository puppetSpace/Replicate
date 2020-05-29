using MediatR;
using Observr;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
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

		public FileService(IDeltaService deltaService
			, IFileRepository fileRespository
			, PathBuilder pathBuilder)
		{
			_deltaService = deltaService;
			_fileRespository = fileRespository;
			_pathBuilder = pathBuilder;
		}

		public async Task<File> CreateNewFile(Guid folderId, System.IO.FileInfo newFile)
		{
			var signature = _deltaService.CreateSignature(newFile.FullName);
			var file = File.Build(newFile,folderId, _pathBuilder.BasePath);

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

	}
}
