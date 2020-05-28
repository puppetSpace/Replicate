﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.EofMessages.Commands.AddToSendEofMessage;
using Pi.Replicate.Application.Files.Commands.MarkFileAsFailed;
using Pi.Replicate.Application.Files.Queries.GetPreviousSignatureOfFile;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
    public class FileDisassemblerService
    {
        private readonly int _sizeofChunkInBytes;
        private readonly ICompressionService _compressionService;
        private readonly PathBuilder _pathBuilder;
        private readonly IDeltaService _deltaService;
        private readonly IMediator _mediator;
		private readonly IWebhookService _webhookService;

		public FileDisassemblerService(IConfiguration configuration
            , ICompressionService compressionService
            , PathBuilder pathBuilder
            , IDeltaService deltaService
            , IMediator mediator
			, IWebhookService webhookService)
        {
            _sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
            _compressionService = compressionService;
            _pathBuilder = pathBuilder;
            _deltaService = deltaService;
            _mediator = mediator;
			_webhookService = webhookService;
		}


        public async Task<EofMessage> ProcessFile(File file, Func<FileChunk,Task> chunkCreatedDelegate)
        {
            var path = _pathBuilder.BuildPath(file.Path);
			EofMessage eofMessage = null;

            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
            {
				try
				{
					if (file.IsNew())
						eofMessage = await ProcessNewFile(file, path, chunkCreatedDelegate);
					else
						eofMessage = await ProcessChangedFile(file, path, chunkCreatedDelegate);

					if(eofMessage is object)
						_webhookService.NotifyFileDisassembled(file);

				}
				catch (Exception ex)
				{
					Log.Error(ex, $"Unexpected error occured while disassembling file '{file.Path}'");
					await HandleFailed(file);
				}
			}
            else
            {
                Log.Warning($"File '{path}' does not exist or is locked. File will not be processed");
				await HandleFailed(file);

			}

            return eofMessage;
        }

		private async Task<EofMessage> ProcessNewFile(File file, string path, Func<FileChunk, Task> chunkCreatedDelegate)
		{
			var sequenceNo = 0;
			Log.Information($"Compressing file '{path}'");
			var pathOfCompressed = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
			await _compressionService.Compress(path, pathOfCompressed);

			Log.Information($"Splitting up '{path}'");

			var sharedmemory = MemoryPool<byte>.Shared.Rent(_sizeofChunkInBytes);
			using (var stream = System.IO.File.OpenRead(pathOfCompressed))
			{
				while ((await stream.ReadAsync(sharedmemory.Memory)) > 0)
				{
					var fileChunk = FileChunk.Build(file.Id, ++sequenceNo, sharedmemory.Memory.ToArray());
					await chunkCreatedDelegate(fileChunk);
				}
			}

			DeleteTempFile(path, pathOfCompressed);

			return await CreateEofMessage(file,sequenceNo);
		}

		private static void DeleteTempFile(string path, string pathOfCompressed)
		{
			Log.Information($"'compressed file of {path}' is being deleted");
			try
			{
				System.IO.File.Delete(pathOfCompressed);
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to delete temp file");
			}
		}

		private async Task<EofMessage> ProcessChangedFile(File file, string path, Func<FileChunk, Task> chunkCreatedDelegate)
        {
            var amountOfChunks = 0;

			var result = await _mediator.Send(new GetPreviousSignatureOfFileQuery() { FileId = file.Id });
			if (!result.WasSuccessful || result.Data.IsEmpty)
			{
				Log.Information($"Unable to process changed file '{file.Path}' due to {(result.WasSuccessful ? "no previous signature found" : "an error while querying the previous signature")}");
				return null;
			}
			else
			{
				Log.Information($"Creating delta of changed file '{file.Path}'");
				var delta = _deltaService.CreateDelta(path, result.Data);
				var deltaSizeOfChunks = delta.Length > _sizeofChunkInBytes ? _sizeofChunkInBytes : delta.Length;

				Log.Information($"Splitting up delta of changed file '{file.Path}'");
				var indexOfSlice = 0;
				var sequenceNo = 0;
				while (indexOfSlice < delta.Length)
				{
					var fileChunk = FileChunk.Build(file.Id, ++sequenceNo, delta.Slice(indexOfSlice, deltaSizeOfChunks));
					await chunkCreatedDelegate(fileChunk);
					indexOfSlice += deltaSizeOfChunks;
					amountOfChunks++;
				}

				return await CreateEofMessage(file, amountOfChunks);
			}
        }

		private async Task<EofMessage> CreateEofMessage(File file, int amountOfChunks)
		{
			Log.Information($"Creating eof message for '{file.Path}'");
			var eofResult = await _mediator.Send(new AddToSendEofMessageCommand { FileId = file.Id, AmountOfChunks = amountOfChunks });
			if (eofResult.WasSuccessful)
				return eofResult.Data;
			else
				return null;
		}

		private async Task HandleFailed(File file)
		{
			await _mediator.Send(new MarkFileAsFailedCommand { FileId = file.Id });
			_webhookService.NotifyFileFailed(file);
		}
	}
}
