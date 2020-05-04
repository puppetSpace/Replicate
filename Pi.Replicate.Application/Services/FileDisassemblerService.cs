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
	//todo errorhandling
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
            int amountOfChunks = 0;

            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
            {
                if (file.IsNew())
                    amountOfChunks = await ProcessNewFile(file, path, chunkCreatedDelegate);
                else
                    amountOfChunks = await ProcessChangedFile(file, path, chunkCreatedDelegate);
            }
            else
            {
                Log.Warning($"File '{path}' does not exist or is locked. File will not be processed");
            }

            Log.Information($"Creating eof message for '{file.Path}'");
            var eofMessage = await _mediator.Send(new AddToSendEofMessageCommand { FileId = file.Id, AmountOfChunks = amountOfChunks});

            return eofMessage;
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
                    var fileChunk = FileChunk.Build(file.Id, ++sequenceNo, sharedmemory.Memory.ToArray());
                    chunkCreatedDelegate(fileChunk);
                }
            }

            Log.Information($"'compressed file of {path}' is being deleted");
            System.IO.File.Delete(pathOfCompressed);


            return sequenceNo;
        }

        private async Task<int> ProcessChangedFile(File file, string path, Action<FileChunk> chunkCreatedDelegate)
        {
            int amountOfChunks = 0;

			var previousSignature = await _mediator.Send(new GetPreviousSignatureOfFileQuery() { FileId = file.Id });

			if (!previousSignature.IsEmpty)
			{

				Log.Information($"Creating delta of changed file '{file.Path}'");
				var delta = _deltaService.CreateDelta(path, previousSignature);
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
			}
            return amountOfChunks;
        }
    }
}
