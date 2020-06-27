using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public interface ITransmissionLink
	{
		Task SendEofMessage(Recipient recipient, EofMessage message);
		Task SendFile(Recipient recipient, Folder folder, File file);
		Task SendFileChunk(Recipient recipient, FileChunk fileChunk);
	}

	public class RestTransmissionLink : ITransmissionLink
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public RestTransmissionLink(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task SendFile(Recipient recipient, Folder folder, File file)
		{
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
			var response =  await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok: true);
			response.EnsureSuccessStatusCode();
		}

		public async Task SendEofMessage(Recipient recipient, EofMessage message)
		{
			var httpClient = _httpClientFactory.CreateClient("default");
			var endpoint = $"{recipient.Address}/api/file/{message.FileId}/eot";
			var eotModel = new EofMessageTransmissionModel { AmountOfChunks = message.AmountOfChunks };
			var response = await httpClient.PostAsync(endpoint, eotModel, throwErrorOnResponseNok: true);
			response.EnsureSuccessStatusCode();
		}

		public async Task SendFileChunk(Recipient recipient, FileChunk fileChunk)
		{
			var httpClient = _httpClientFactory.CreateClient("default");

			var address = $"{recipient.Address}/api/file/{fileChunk.FileId}/chunk/{fileChunk.SequenceNo}/{Environment.MachineName}";
			var content = new ByteArrayContent(fileChunk.Value);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			var response = await httpClient.PostAsync(address, content);
			response.EnsureSuccessStatusCode();
		}

	}

	public class GrpcTransmissionLink : ITransmissionLink
	{
		public async Task SendFile(Recipient recipient, Folder folder, File file)
		{

		}

		public async Task SendEofMessage(Recipient recipient, EofMessage message)
		{

		}

		public async Task SendFileChunk(Recipient recipient, FileChunk fileChunk)
		{

		}

	}
}
