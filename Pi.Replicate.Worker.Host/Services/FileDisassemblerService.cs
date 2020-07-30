using Microsoft.Extensions.Configuration;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class FileDisassemblerService
	{
		private readonly int _sizeofChunkInBytes;
		private readonly IWebhookService _webhookService;
		private readonly IFileRepository _fileRepository;
		private readonly IEofMessageRepository _eofMessageRepository;

		public FileDisassemblerService(IConfiguration configuration
			, IWebhookService webhookService
			, IFileRepository fileRepository
			, IEofMessageRepository eofMessageRepository)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_webhookService = webhookService;
			_fileRepository = fileRepository;
			_eofMessageRepository = eofMessageRepository;
		}


		public async Task<EofMessage> ProcessFile(File file, IChunkWriter chunkWriter)
		{
			var path = file.GetFullPath();
			EofMessage eofMessage = null;

			if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				try
				{
					if (file.IsNew())
						eofMessage = await ProcessNewFile(file, chunkWriter);
					else
						eofMessage = await ProcessChangedFile(file, chunkWriter);

					if (eofMessage is object)
						_webhookService.NotifyFileDisassembled(file);

				}
				catch (Exception ex)
				{
					WorkerLog.Instance.Error(ex, $"Unexpected error occured while disassembling file '{file.RelativePath}'");
					await HandleFailed(file);
				}
			}
			else
			{
				WorkerLog.Instance.Warning($"File '{path}' does not exist or is locked. File will not be processed");
				await HandleFailed(file);

			}

			return eofMessage;
		}

		private async Task<EofMessage> ProcessNewFile(File file, IChunkWriter chunkWriter)
		{
			var sequenceNo = 0;
			WorkerLog.Instance.Information($"Compressing file '{file.RelativePath}'");
			var pathOfCompressed = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			await file.CompressTo(pathOfCompressed);

			WorkerLog.Instance.Information($"Splitting up '{file.RelativePath}'");

			var buffer = new byte[_sizeofChunkInBytes];
			using (var stream = System.IO.File.OpenRead(pathOfCompressed))
			{
				while ((await stream.ReadAsync(buffer,0,buffer.Length)) > 0)
				{
					var fileChunk = new FileChunk(file.Id, ++sequenceNo, buffer);
					await chunkWriter.Push(fileChunk);
				}
			}

			DeleteTempFile(pathOfCompressed);

			return await CreateEofMessage(file, sequenceNo);
		}

		private static void DeleteTempFile(string pathOfCompressed)
		{
			WorkerLog.Instance.Information($"'compressed file of {pathOfCompressed}' is being deleted");
			try
			{
				System.IO.File.Delete(pathOfCompressed);
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Warning(ex, "Failed to delete temp file");
			}
		}

		private async Task<EofMessage> ProcessChangedFile(File file, IChunkWriter chunkWriter)
		{
			var amountOfChunks = 0;

			var result = await _fileRepository.GetSignatureOfPreviousFile(file.Id);
			if (!result.WasSuccessful || result.Data.Length == 0)
			{
				WorkerLog.Instance.Information($"Unable to process changed file '{file.RelativePath}' due to {(result.WasSuccessful ? "no previous signature found" : "an error while querying the previous signature")}");
				return null;
			}
			else
			{
				WorkerLog.Instance.Information($"Creating delta of changed file '{file.RelativePath}'");
				var delta = new ReadOnlyMemory<byte>(file.CreateDelta(result.Data));
				var deltaSizeOfChunks = delta.Length > _sizeofChunkInBytes ? _sizeofChunkInBytes : delta.Length;

				WorkerLog.Instance.Information($"Splitting up delta of changed file '{file.RelativePath}'");
				var indexOfSlice = 0;
				var sequenceNo = 0;
				while (indexOfSlice < delta.Length)
				{
					var fileChunk = new FileChunk(file.Id, ++sequenceNo, delta.Slice(indexOfSlice, deltaSizeOfChunks).ToArray());
					await chunkWriter.Push(fileChunk);
					indexOfSlice += deltaSizeOfChunks;
					amountOfChunks++;
				}

				return await CreateEofMessage(file, amountOfChunks);
			}
		}

		private async Task<EofMessage> CreateEofMessage(File file, int amountOfChunks)
		{
			WorkerLog.Instance.Information($"Creating eof message for '{file.RelativePath}'");
			var eofMessage = new EofMessage(file.Id, amountOfChunks);
			var eofResult = await _eofMessageRepository.AddEofMessage(eofMessage);
			if (eofResult.WasSuccessful)
				return eofMessage;
			else
				return null;
		}

		private async Task HandleFailed(File file)
		{
			await _fileRepository.UpdateFileAsFailed(file.Id);
			_webhookService.NotifyFileFailed(file);
		}
	}

	public interface IChunkWriter
	{
		Task Push(FileChunk fileChunk);
	}

	public class ChunkWriter : IChunkWriter
	{
		private readonly ICollection<Recipient> _recipients;
		private readonly ChannelWriter<(Recipient recipient, FileChunk filechunk)> _writer;

		public ChunkWriter(ICollection<Recipient> recipients, System.Threading.Channels.ChannelWriter<(Recipient recipient, FileChunk filechunk)> writer)
		{
			_recipients = recipients;
			_writer = writer;
		}

		public async Task Push(FileChunk fileChunk)
		{
			foreach (var recipient in _recipients)
			{
				if (await _writer.WaitToWriteAsync())
					await _writer.WriteAsync((recipient, fileChunk));
			}
		}
	}
}
