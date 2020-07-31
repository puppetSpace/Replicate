using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
			var response = await httpClient.PostAsync(endpoint, fileTransmissionModel, throwErrorOnResponseNok: true);
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
			var content = new ByteArrayContent(fileChunk.GetValue());
			content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			var response = await httpClient.PostAsync(address, content);
			response.EnsureSuccessStatusCode();
		}

	}

	public class GrpcTransmissionLink : ITransmissionLink
	{
		private static ChannelStore _channelStore = new ChannelStore();

		public async Task SendFile(Recipient recipient, Folder folder, File file)
		{
			var client = new Transmitter.TransmitterClient(_channelStore.Get(recipient.Address));
			var result = await client.SendFileAsync(new FileTransmissionRequest
			{
				Id = file.Id.ToString(),
				Name = file.Name,
				LastModifiedDate = Timestamp.FromDateTime(file.LastModifiedDate),
				Path = file.Path,
				Size = file.Size,
				Version = file.Version,
				FolderName = folder.Name,
				Host = Environment.MachineName,
			});

			if (!result.IsSuccessful)
				throw new InvalidOperationException($"'{nameof(SendFile)}' received a negative 'IsSuccessful' from '{recipient.Name}'");
		}

		public async Task SendEofMessage(Recipient recipient, EofMessage message)
		{
			var client = new Transmitter.TransmitterClient(_channelStore.Get(recipient.Address));
			var result = await client.SendEofMessageAsync(new EofMessageTransmissionRequest
			{ 
				FileId = message.FileId.ToString(), 
				AmountOfChunks = message.AmountOfChunks 
			});

			if (!result.IsSuccessful)
				throw new InvalidOperationException($"'{nameof(SendEofMessage)}' received a negative 'IsSuccessful' from '{recipient.Name}'");
		}

		public async Task SendFileChunk(Recipient recipient, FileChunk fileChunk)
		{
			var client = new Transmitter.TransmitterClient(_channelStore.Get(recipient.Address));
			var result = await client.SendFileChunkAsync(new FileChunkTransmissionRequest
			{
				FileId = fileChunk.FileId.ToString(),
				SequenceNo = fileChunk.SequenceNo,
				Host = Environment.MachineName,
				Value = ByteString.CopyFrom(fileChunk.GetValue())
			});

			if (!result.IsSuccessful)
				throw new InvalidOperationException($"'{nameof(SendFileChunk)}' received a negative 'IsSuccessful' from '{recipient.Name}'");
		}

		private class ChannelStore
		{
			private Dictionary<string, GrpcChannel> _channels = new Dictionary<string, GrpcChannel>();


			public GrpcChannel Get(string url)
			{
				if (_channels.ContainsKey(url))
				{
					return _channels[url];
				}
				else
				{
					var channel = GrpcChannel.ForAddress(url);
					_channels.Add(url, channel);
					return channel;
				}
			}

		}

	}
}
