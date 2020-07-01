using Grpc.Core;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Hubs
{
	public class TransmissionHub : Transmitter.TransmitterBase
	{
		private readonly FileService _fileService;
		private readonly IEofMessageRepository _eofMessageRepository;
		private readonly FileChunkService _fileChunkService;

		private static readonly FileChunkTransmissionResponse _successFullFileChunkTransmissionResponse = new FileChunkTransmissionResponse { IsSuccessful = true };
		private static readonly FileChunkTransmissionResponse _failedFileChunkTransmissionResponse = new FileChunkTransmissionResponse { IsSuccessful = false };
		private static readonly EofMessageTransmissionResponse _successFullEofMessageTransmissionResponse = new EofMessageTransmissionResponse { IsSuccessful = true };
		private static readonly EofMessageTransmissionResponse _failedEofMessageTransmissionResponse = new EofMessageTransmissionResponse { IsSuccessful = false };
		private static readonly FileTransmissionResponse _successFullFileTransmissionResponse = new FileTransmissionResponse { IsSuccessful = true };
		private static readonly FileTransmissionResponse _failedEFileTransmissionResponse = new FileTransmissionResponse { IsSuccessful = false };

		public TransmissionHub(FileService fileService, IEofMessageRepository eofMessageRepository, FileChunkService fileChunkService)
		{
			_fileService = fileService;
			_eofMessageRepository = eofMessageRepository;
			_fileChunkService = fileChunkService;
		}

		public override async Task<FileTransmissionResponse> SendFile(FileTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"File data received from {request.Host}");
			var result = await _fileService.AddReceivedFile(Guid.Parse(request.Id), request.FolderName, request.Name, request.Size, request.Version, request.LastModifiedDate.ToDateTime(), request.Path, request.Host);
			return result.WasSuccessful
				? _successFullFileTransmissionResponse
				: _failedEFileTransmissionResponse;
		}

		public override async Task<EofMessageTransmissionResponse> SendEofMessage(EofMessageTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"Eof message received from {request.Host}");
			var result = await _eofMessageRepository.AddReceivedEofMessage(EofMessage.Build(Guid.Parse(request.FileId), request.AmountOfChunks));
			return result.WasSuccessful
				? _successFullEofMessageTransmissionResponse
				: _failedEofMessageTransmissionResponse;
		}

		public override async Task<FileChunkTransmissionResponse> SendFileChunk(FileChunkTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"Filechunk received from {request.Host}");
			var result = await _fileChunkService.AddReceivedFileChunk(Guid.Parse(request.FileId), request.SequenceNo, request.Value.ToByteArray(), request.Host, DummyAdress.Create(request.Host));
			return result.WasSuccessful
				? _successFullFileChunkTransmissionResponse
				: _failedFileChunkTransmissionResponse;
		}
	}
}
