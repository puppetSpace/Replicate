using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.FailedTransmissions.Commands.AddFailedTransmission;
using Pi.Replicate.Application.Folders.Queries.GetFolder;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Pi.Replicate.TransmissionResults.Commands.AddTransmissionResult;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class TransmissionService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;

		public TransmissionService(IHttpClientFactory httpClientFactory,IMapper mapper, IMediator mediator)
		{
			_httpClientFactory = httpClientFactory;
			_mapper = mapper;
			_mediator = mediator;
		}

		public async Task<bool> SendFile(Folder folder,File file, Recipient recipient)
		{
			try
			{
				Log.Information($"Sending '{file.Path}' metadata to {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file";
				var fileTransmissionModel = _mapper.Map<FileTransmissionModel>(file);
				fileTransmissionModel.FolderName = folder.Name;
				await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok: true);
				return true;
			}
			catch (System.Exception ex)
			{
				Log.Error(ex, $"Failed to send file metadata of '{file.Path}' to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				await _mediator.Send(new AddFailedFileTransmissionCommand { FileId = file.Id, RecipientId = recipient.Id });
				return false;
			}
		}

		public async Task<bool> SendEofMessage(EofMessage message, Recipient recipient)
		{
			try
			{
				Log.Information($"Sending Eot message for file '{message.File.Path}' to {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file/{message.FileId}/eot";
				var eotModel = _mapper.Map<EofMessageTransmissionModel>(message);
				await httpClient.PostAsync(endpoint, eotModel, throwErrorOnResponseNok: true);
				return true;
			}
			catch (System.Exception ex)
			{
				Log.Error(ex, $"Failed to send Eof message of '{message.File.Path}' to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				await _mediator.Send(new AddFailedEofMessageTransmissionCommand { EofMessage = message, RecipientId = recipient.Id });
				return false;
			}
		}

		public async Task<bool> SendFileChunk(FileChunk fileChunk, Recipient recipient)
		{
			Log.Information($"Sending chunk '{fileChunk.SequenceNo}' to '{recipient.Name}'");
			try
			{
				var httpClient = _httpClientFactory.CreateClient("default");
				var chunkModel = _mapper.Map<FileChunkTransmissionModel>(fileChunk);
				await httpClient.PostAsync($"{recipient.Address}/api/file/{fileChunk.FileId}/chunk", chunkModel, throwErrorOnResponseNok: true);
				await _mediator.Send(new AddTransmissionResultCommand{FileId = fileChunk.FileId, FileChunkSequenceNo = fileChunk.SequenceNo, RecipientId = recipient.Id});
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to send chunk to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				await _mediator.Send(new AddFailedFileChunkTransmissionCommand { FileChunk = fileChunk, RecipientId = recipient.Id });
				return false;
			}
		}

	}
}
