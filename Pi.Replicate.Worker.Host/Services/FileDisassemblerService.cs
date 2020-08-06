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
		private readonly IFileRepository _fileRepository;
		private readonly IEofMessageRepository _eofMessageRepository;
		private readonly File _file;
		private readonly IChunkWriter _chunkWriter;

		public FileDisassemblerService(int sizeofChunksInBytes
			, IFileRepository fileRepository
			, IEofMessageRepository eofMessageRepository
			, File file
			, IChunkWriter chunkWriter)
		{
			_sizeofChunkInBytes = sizeofChunksInBytes;
			_fileRepository = fileRepository;
			_eofMessageRepository = eofMessageRepository;
			_file = file;
			_chunkWriter = chunkWriter;
		}


		public async Task<EofMessage> ProcessFile()
		{
			var path = _file.GetFullPath();

			EofMessage eofMessage = null;
			if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				try
				{
					if (_file.IsNew())
						eofMessage = await ProcessNewFile();
					else
						eofMessage = await ProcessChangedFile();
				}
				catch (Exception ex)
				{
					WorkerLog.Instance.Error(ex, $"Unexpected error occured while disassembling file '{_file.Path}'");
				}
			}
			else
			{
				WorkerLog.Instance.Warning($"File '{path}' does not exist or is locked. File will not be processed");
			}

			if(eofMessage is null)
			{
				await _fileRepository.UpdateFileAsFailed(_file.Id);
			}

			return eofMessage;
		}

		private async Task<EofMessage> ProcessNewFile()
		{
			var sequenceNo = 0;
			WorkerLog.Instance.Information($"Compressing file '{_file.Path}'");
			var pathOfCompressed = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			await _file.CompressTo(pathOfCompressed);

			WorkerLog.Instance.Information($"Splitting up '{_file.Path}'");

			var buffer = new byte[_sizeofChunkInBytes];
			using (var stream = System.IO.File.OpenRead(pathOfCompressed))
			{
				int bytesRead = 0;
				while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					var fileChunk = new FileChunk(_file.Id, ++sequenceNo, buffer[0..bytesRead]);
					await _chunkWriter.Push(fileChunk);
				}
			}

			DeleteTempFile(pathOfCompressed);

			return await CreateEofMessage(sequenceNo);
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

		private async Task<EofMessage> ProcessChangedFile()
		{
			var amountOfChunks = 0;

			var result = await _fileRepository.GetSignatureOfPreviousFile(_file.Id);
			if (!result.WasSuccessful || result.Data.Length == 0)
			{
				WorkerLog.Instance.Information($"Unable to process changed file '{_file.Path}' due to {(result.WasSuccessful ? "no previous signature found" : "an error while querying the previous signature")}");
				return null;
			}
			else
			{
				WorkerLog.Instance.Information($"Creating delta of changed file '{_file.Path}'");
				var delta = _file.CreateDelta(result.Data);
				var deltaSizeOfChunks = delta.Length > _sizeofChunkInBytes ? _sizeofChunkInBytes : delta.Length;

				WorkerLog.Instance.Information($"Splitting up delta of changed file '{_file.Path}'");
				var indexOfSlice = 0;
				var sequenceNo = 0;
				while (indexOfSlice < delta.Length)
				{
					var fileChunk = new FileChunk(_file.Id, ++sequenceNo, delta[indexOfSlice..deltaSizeOfChunks]);
					await _chunkWriter.Push(fileChunk);
					indexOfSlice += deltaSizeOfChunks;
					amountOfChunks++;
				}

				return await CreateEofMessage(amountOfChunks);
			}
		}

		private async Task<EofMessage> CreateEofMessage(int amountOfChunks)
		{
			WorkerLog.Instance.Information($"Creating eof message for '{_file.Path}'");
			var eofMessage = new EofMessage(_file.Id, amountOfChunks);
			var eofResult = await _eofMessageRepository.AddEofMessage(eofMessage);
			if (eofResult.WasSuccessful)
				return eofMessage;
			else
				return null;
		}

	}

	public class FileDisassemblerServiceFactory
	{
		private readonly IFileRepository _fileRepository;
		private readonly IEofMessageRepository _eofMessageRepository;
		private readonly int _sizeofChunkInBytes;
		public FileDisassemblerServiceFactory(IConfiguration configuration
			, IFileRepository fileRepository
			, IEofMessageRepository eofMessageRepository)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_fileRepository = fileRepository;
			_eofMessageRepository = eofMessageRepository;
		}

		public FileDisassemblerService Get(File file, IChunkWriter chunkWriter)
		{
			return new FileDisassemblerService(_sizeofChunkInBytes, _fileRepository, _eofMessageRepository, file, chunkWriter);
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
