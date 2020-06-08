﻿using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class TransmissionService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly TransmissionRepository _transmissionRepository;

		public TransmissionService(IHttpClientFactory httpClientFactory, TransmissionRepository transmissionRepository)
		{
			_httpClientFactory = httpClientFactory;
			_transmissionRepository = transmissionRepository;
		}

		public async Task<bool> SendFile(Folder folder, File file, Recipient recipient)
		{
			var canContinue = true;
			try
			{
				WorkerLog.Instance.Information($"Sending '{file.Path}' metadata to {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file";
				var fileTransmissionModel = new FileTransmissionModel
				{
					Id = file.Id,
					LastModifiedDate = file.LastModifiedDate,
					Name = file.Name,
					Path = file.Path,
					Size = file.Size,
					Version = file.Version,
					FolderName = folder.Name,
					Host = Environment.MachineName
				};
				var response = await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok: true);
			}
			catch (System.Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send file metadata of '{file.Path}' to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedFileTransmission(file.Id, recipient.Id);
				canContinue = result.WasSuccessful;
			}

			return canContinue;
		}

		public async Task<bool> SendEofMessage(EofMessage message, Recipient recipient)
		{
			var canContinue = true;
			try
			{
				WorkerLog.Instance.Information($"Sending Eot message to {recipient.Name}");

				var httpClient = _httpClientFactory.CreateClient("default");
				var endpoint = $"{recipient.Address}/api/file/{message.FileId}/eot";
				var eotModel = new EofMessageTransmissionModel { AmountOfChunks = message.AmountOfChunks };
				await httpClient.PostAsync(endpoint, eotModel, throwErrorOnResponseNok: true);
			}
			catch (System.Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send Eof message to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedEofMessageTransmission(message.Id, recipient.Id);
				canContinue = result.WasSuccessful;
			}
			return canContinue;
		}

		public async Task<bool> SendFileChunk(FileChunk fileChunk, Recipient recipient)
		{
			var canContinue = true;
			var fileChunkValue = fileChunk.Value;
			try
			{
				WorkerLog.Instance.Information($"Sending chunk '{fileChunk.SequenceNo}' to '{recipient.Name}'");
				var httpClient = _httpClientFactory.CreateClient("default");

				var address = $"{recipient.Address}/api/file/{fileChunk.FileId}/chunk/{fileChunk.SequenceNo}/{Environment.MachineName}";
				var content = new ByteArrayContent(fileChunkValue);
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				var response = await httpClient.PostAsync(address, content);
				response.EnsureSuccessStatusCode();
				await _transmissionRepository.AddTransmissionResult(fileChunk.FileId, recipient.Id, fileChunk.SequenceNo, FileSource.Local);
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send chunk to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedFileChunkTransmission(fileChunk.Id, fileChunk.FileId, recipient.Id, fileChunk.SequenceNo, fileChunkValue);
				canContinue = result.WasSuccessful;
			}
			return canContinue;
		}

	}
}
