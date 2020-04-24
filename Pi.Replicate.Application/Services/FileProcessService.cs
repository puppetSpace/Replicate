﻿using Microsoft.Extensions.Configuration;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class FileProcessService
	{
		private readonly int _sizeofChunkInBytes;
		private readonly CompressionService _compressionService;
		private readonly PathBuilder _pathBuilder;
		private readonly DeltaService _deltaService;

		public FileProcessService(IConfiguration configuration
			, CompressionService compressionService
			, PathBuilder pathBuilder
			, DeltaService deltaService)
		{
			_sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
			_compressionService = compressionService;
			_pathBuilder = pathBuilder;
			_deltaService = deltaService;
		}


		public async Task<EofMessage> ProcessFile(File file, Action<FileChunk> chunkCreatedDelegate)
		{
			var path = _pathBuilder.BuildPath(file.Path);
			int amountOfChunks = 0;

			if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
			{
				if (file.IsNew())
					amountOfChunks = await ProcessNewFile(file, path, chunkCreatedDelegate);
				else
					amountOfChunks = ProcessChangedFile(file, path, chunkCreatedDelegate);
			}
			else
			{
				Log.Warning($"File '{path}' does not exist or is locked. File will not be processed");
			}

			return EofMessage.Build(file, amountOfChunks);
		}

		private async Task<int> ProcessNewFile(File file, string path, Action<FileChunk> chunkCreatedDelegate)
		{
			int sequenceNo = 0;
			Log.Information($"Compressing file '{path}'");
			var pathOfCompressed = await _compressionService.CompressFileToTempFile(path);

			Log.Information($"Splitting up '{path}'");

			var sharedmemory = MemoryPool<byte>.Shared.Rent(_sizeofChunkInBytes);
			using (var stream = System.IO.File.OpenRead(pathOfCompressed))
			{
				while ((await stream.ReadAsync(sharedmemory.Memory)) > 0)
				{
					var fileChunk = FileChunk.Build(file.Id, sequenceNo, sharedmemory.Memory, ChunkSource.FromNewFile);
					chunkCreatedDelegate(fileChunk);
				}
			}

			Log.Information($"'compressed file of {path}' is being deleted");
			System.IO.File.Delete(pathOfCompressed);


			return sequenceNo;
		}

		private int ProcessChangedFile(File file, string path, Action<FileChunk> chunkCreatedDelegate)
		{
			int amountOfChunks = 0;
			Log.Information($"Creating delta of changed file '{file.Path}'");

			var delta = _deltaService.CreateDelta(path, file.Signature);
			var deltaSizeOfChunks = delta.Length > _sizeofChunkInBytes ? _sizeofChunkInBytes : delta.Length;

			Log.Information($"Splitting up delta of changed file '{file.Path}'");
			var indexOfSlice = 0;
			double sequenceNo = file.Version;
			while (indexOfSlice < delta.Length)
			{
				var fileChunk = FileChunk.Build(file.Id, sequenceNo += 0.0001, delta.Slice(indexOfSlice, deltaSizeOfChunks), ChunkSource.FromNewFile);
				chunkCreatedDelegate(fileChunk);
				indexOfSlice += deltaSizeOfChunks;
				amountOfChunks++;
			}
			return amountOfChunks;
		}
	}
}
