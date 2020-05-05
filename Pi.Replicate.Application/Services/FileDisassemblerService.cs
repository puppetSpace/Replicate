using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.EofMessages.Commands.AddToSendEofMessage;
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
        private readonly CompressionService _compressionService;
        private readonly PathBuilder _pathBuilder;
        private readonly DeltaService _deltaService;
        private readonly IMediator _mediator;

        public FileDisassemblerService(IConfiguration configuration
            , CompressionService compressionService
            , PathBuilder pathBuilder
            , DeltaService deltaService
            , IMediator mediator)
        {
            _sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
            _compressionService = compressionService;
            _pathBuilder = pathBuilder;
            _deltaService = deltaService;
            _mediator = mediator;
        }


        public async Task<EofMessage> ProcessFile(File file, Action<FileChunk> chunkCreatedDelegate)
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
				}
				catch (Exception ex)
				{
					Log.Error(ex, $"Unexpected error occured while disassembling file '{file.Path}'");
				}
			}
            else
            {
                Log.Warning($"File '{path}' does not exist or is locked. File will not be processed");
            }

            return eofMessage;
        }

		private async Task<EofMessage> ProcessNewFile(File file, string path, Action<FileChunk> chunkCreatedDelegate)
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
					var fileChunk = FileChunk.Build(file.Id, ++sequenceNo, sharedmemory.Memory.ToArray());
					chunkCreatedDelegate(fileChunk);
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

		private async Task<EofMessage> ProcessChangedFile(File file, string path, Action<FileChunk> chunkCreatedDelegate)
        {
            int amountOfChunks = 0;

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
				int sequenceNo = 0;
				while (indexOfSlice < delta.Length)
				{
					var fileChunk = FileChunk.Build(file.Id, ++sequenceNo, delta.Slice(indexOfSlice, deltaSizeOfChunks));
					chunkCreatedDelegate(fileChunk);
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
	}
}
