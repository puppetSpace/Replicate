using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Processing.Transmission
{
	public class FileReceivedAction
    {
		private readonly FolderRepository _folderRepository;
		private readonly RecipientRepository _recipientRepository;
		private readonly IFileRepository _fileRespository;
		private readonly PathBuilder _pathBuilder;
		private readonly CommunicationProxy _communicationProxy;

		public FileReceivedAction(FolderRepository folderRepository
			, RecipientRepository recipientRepository
			, IFileRepository fileRespository
			, PathBuilder pathBuilder
			, CommunicationProxy communicationProxy)
		{
			_folderRepository = folderRepository;
			_recipientRepository = recipientRepository;
			_fileRespository = fileRespository;
			_pathBuilder = pathBuilder;
			_communicationProxy = communicationProxy;
		}


		public async Task<bool> Execute(Guid fileId, string folderName, string name,long size,int version, DateTime lastModifiedDate,string path,string host)
		{
			var folderCreation = await AddFolder(folderName);
			if (folderCreation.WasSuccessful)
			{
				var fileResult = await _fileRespository.AddNewFile(
					new File
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

				return fileResult.WasSuccessful && recipientResult.WasSuccessful;
			}

			return false;
		}

		private async Task<Result<Guid>> AddFolder(string folderName)
		{
			var folderAddResult = await _folderRepository.AddFolder(folderName);
			if (folderAddResult.WasSuccessful)
			{
				var folderPath = _pathBuilder.BuildPath(folderName);
				if (!System.IO.Directory.Exists(folderPath))
					System.IO.Directory.CreateDirectory(folderPath);
				_communicationProxy.SendNewFolderAddedNotification(new FolderAddedNotification { Id = folderAddResult.Data, Name = folderName }).Forget();

				return folderAddResult;
			}
			else
			{
				return Result<Guid>.Failure();
			}
		}
	}
}
