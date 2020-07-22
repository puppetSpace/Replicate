﻿using Grpc.Core;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Hubs
{
	internal class TransmissionHub : Transmitter.TransmitterBase
	{
		private readonly TransmissionActions _transmissionActions;

		public TransmissionHub(TransmissionActions transmissionActions)
		{
			_transmissionActions = transmissionActions;
		}

		public override async Task<FileTransmissionResponse> SendFile(FileTransmissionRequest request, ServerCallContext context)
		{
			return await _transmissionActions.SendFile(request);
		}

		public override async Task<EofMessageTransmissionResponse> SendEofMessage(EofMessageTransmissionRequest request, ServerCallContext context)
		{
			return await _transmissionActions.SendEofMessage(request);
		}

		public override async Task<FileChunkTransmissionResponse> SendFileChunk(FileChunkTransmissionRequest request, ServerCallContext context)
		{
			await _transmissionActions.SendFileChunk(request);
			return new FileChunkTransmissionResponse();
		}

		internal class TransmissionActions
		{
			private readonly FileService _fileService;
			private readonly IEofMessageRepository _eofMessageRepository;
			private readonly IFileChunkRepository _fileChunkRepository;

			private static readonly FileChunkTransmissionResponse _successFullFileChunkTransmissionResponse = new FileChunkTransmissionResponse { IsSuccessful = true };
			private static readonly FileChunkTransmissionResponse _failedFileChunkTransmissionResponse = new FileChunkTransmissionResponse { IsSuccessful = false };
			private static readonly EofMessageTransmissionResponse _successFullEofMessageTransmissionResponse = new EofMessageTransmissionResponse { IsSuccessful = true };
			private static readonly EofMessageTransmissionResponse _failedEofMessageTransmissionResponse = new EofMessageTransmissionResponse { IsSuccessful = false };
			private static readonly FileTransmissionResponse _successFullFileTransmissionResponse = new FileTransmissionResponse { IsSuccessful = true };
			private static readonly FileTransmissionResponse _failedEFileTransmissionResponse = new FileTransmissionResponse { IsSuccessful = false };

			public TransmissionActions(FileService fileService, IEofMessageRepository eofMessageRepository, IFileChunkRepository fileChunkRepository)
			{
				_fileService = fileService;
				_eofMessageRepository = eofMessageRepository;
				_fileChunkRepository = fileChunkRepository;
			}

			public async Task<FileTransmissionResponse> SendFile(FileTransmissionRequest request)
			{
				WorkerLog.Instance.Information($"File data received from {request.Host}");
				var result = await _fileService.AddReceivedFile(Guid.Parse(request.Id), request.FolderName, request.Name, request.Size, request.Version, request.LastModifiedDate.ToDateTime(), request.Path, request.Host);
				return result.WasSuccessful
					? _successFullFileTransmissionResponse
					: _failedEFileTransmissionResponse;
			}

			public async Task<EofMessageTransmissionResponse> SendEofMessage(EofMessageTransmissionRequest request)
			{
				WorkerLog.Instance.Information($"Eof message received from {request.Host}");
				var result = await _eofMessageRepository.AddReceivedEofMessage(EofMessage.Build(Guid.Parse(request.FileId), request.AmountOfChunks));
				return result.WasSuccessful
					? _successFullEofMessageTransmissionResponse
					: _failedEofMessageTransmissionResponse;
			}

			public async Task<FileChunkTransmissionResponse> SendFileChunk(FileChunkTransmissionRequest request)
			{
				WorkerLog.Instance.Information($"Filechunk received from {request.Host}");
				var result = await _fileChunkRepository.AddReceivedFileChunk(Guid.Parse(request.FileId), request.SequenceNo, request.Value.ToByteArray(), request.Host, DummyAdress.Create(request.Host)); ;
				return result.WasSuccessful
					? _successFullFileChunkTransmissionResponse
					: _failedFileChunkTransmissionResponse;
			}
		}
	}


}
