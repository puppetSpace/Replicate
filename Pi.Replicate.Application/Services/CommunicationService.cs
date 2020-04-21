using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Pi.Replicate.Application.FileChanges.Commands.AddFaileFileChange;
using Pi.Replicate.Application.FileChanges.Models;
using Pi.Replicate.Application.Files.Commands.AddFailedFile;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Application.Folders.Queries.GetFolderName;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class CommunicationService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMediator _mediator;
		private readonly IMapper _mapper;

		public CommunicationService(IHttpClientFactory httpClientFactory, IMediator mediator, IMapper mapper)
		{
			_httpClientFactory = httpClientFactory;
			_mediator = mediator;
			_mapper = mapper;
		}

		public async Task<bool> SendFile(File file, Recipient recipient)
		{
			try
			{
				Log.Information($"Sending '{file.Path}' metadata to {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file";
				var folderName = await _mediator.Send(new GetFolderNameQuery { FolderId = file.FolderId });
				var fileTransmissionModel = _mapper.Map<FileTransmissionModel>(file);
				fileTransmissionModel.FolderName = folderName;
				await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok: true);
				return true;
			}
			catch (System.Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
			{
				Log.Error(ex, $"Failed to send file metadata of '{file.Path}' to '{recipient.Name}'. Adding file to FailedFiles and retrying later");
				await _mediator.Send(new AddFailedFileCommand { File = file, Recipient = recipient });
				return false;
			}
		}

		public async Task<bool> SendFileChange(FileChange fileChange, Recipient recipient)
		{
			try
			{
				Log.Information($"Sending changes to metadata of '{fileChange.File.Path}' to  {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/filechange";
				var fileChangeTransmissionModel = _mapper.Map<FileChangeTransmissionModel>(fileChange);
				fileChangeTransmissionModel.FileSignature = fileChange.File.Signature;
				fileChangeTransmissionModel.FilePath = fileChange.File.Path;
				fileChangeTransmissionModel.FileSize = fileChange.File.Size;
				await httpClient.PostAsync(endpoint, fileChangeTransmissionModel, throwErrorOnResponseNok: true);
				return true;
			}
			catch (System.Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
			{
				Log.Error(ex, $"Failed to send filechange metadata of '{fileChange.File.Path}' to '{recipient.Name}'. Adding change to FailedFileChanges and retrying later");
				await _mediator.Send(new AddFailedFileChangeCommand { FileChange = fileChange, Recipient = recipient });
				return false;
			}
		}

	}
}
