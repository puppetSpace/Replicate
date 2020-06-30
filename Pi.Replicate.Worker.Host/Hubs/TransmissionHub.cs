using Grpc.Core;
using Pi.Replicate.Worker.Host.Processing.Transmission;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Hubs
{
	public class TransmissionHub : Transmitter.TransmitterBase
	{
		private readonly TransmissionActionFactory _transmissionActionFactory;

		public TransmissionHub(TransmissionActionFactory transmissionActionFactory)
		{
			_transmissionActionFactory = transmissionActionFactory;
		}

		public override async Task<FileTransmissionResponse> SendFile(FileTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"File data received from {request.Host}");
			var isSuccessful = await _transmissionActionFactory
				.GetForFileReceived()
				.Execute(Guid.Parse(request.Id), request.FolderName, request.Name, request.Size, request.Version, request.LastModifiedDate.ToDateTime(), request.Path, request.Host);
			return new FileTransmissionResponse { IsSuccessful = isSuccessful };
		}

		public override async Task<EofMessageTransmissionResponse> SendEofMessage(EofMessageTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"Eof message received from {request.Host}");
			var isSuccessful = await _transmissionActionFactory
				.GetForEofMessageReceived()
				.Execute(Guid.Parse(request.FileId), request.AmountOfChunks);
			return new EofMessageTransmissionResponse { IsSuccessful = isSuccessful };
		}

		public override async Task<FileChunkTransmissionResponse> SendFileChunk(FileChunkTransmissionRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"Filechunk received from {request.Host}");
			var isSuccessful = await _transmissionActionFactory
				.GetForFileChunkReceived()
				.Execute(Guid.Parse(request.FileId), request.SequenceNo, request.Value.ToByteArray(), request.Host);
			return new FileChunkTransmissionResponse { IsSuccessful = isSuccessful };
		}
	}
}
