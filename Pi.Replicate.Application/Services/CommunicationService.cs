using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
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
				Log.Information($"Sending '{file.Path}' metadata to {recipient.Name} so that the file can be created remote");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file";
				var folderName = await _mediator.Send(new GetFolderNameQuery { FolderId = file.FolderId });
				var fileTransmissionModel = _mapper.Map<FileTransmissionModel>(file);
				fileTransmissionModel.FolderName = folderName;
				await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok:true);
				return true;
			}
			catch (System.Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
			{
				Log.Error(ex, $"Failed to send file metadata of '{file.Path}' to '{recipient.Name}'. Adding file to FailedFiles and retrying later");
				await _mediator.Send(new AddFailedFileCommand { File = file, Recipient = recipient });
				return false;
			}
		}

    }
}
