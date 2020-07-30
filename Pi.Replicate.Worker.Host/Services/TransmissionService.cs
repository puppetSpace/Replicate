using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class TransmissionService
	{
		private readonly ITransmissionLink _transmissionLink;
		private readonly ITransmissionRepository _transmissionRepository;

		public TransmissionService(ITransmissionLink transmissionLink, ITransmissionRepository transmissionRepository)
		{
			_transmissionLink = transmissionLink;
			_transmissionRepository = transmissionRepository;
		}

		public async Task<bool> SendFile(Recipient recipient, Folder folder, File file)
		{
			var canContinue = true;
			try
			{
				WorkerLog.Instance.Information($"Sending '{file.RelativePath}' metadata to {recipient.Name}");
				await _transmissionLink.SendFile(recipient, folder, file);
			}
			catch (System.Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send file metadata of '{file.RelativePath}' to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedFileTransmission(file.Id, recipient.Id);
				canContinue = result.WasSuccessful;
			}

			return canContinue;
		}

		public async Task<bool> SendEofMessage(Recipient recipient,EofMessage message)
		{
			var canContinue = true;
			try
			{
				WorkerLog.Instance.Information($"Sending Eot message to {recipient.Name}");
				await _transmissionLink.SendEofMessage(recipient, message);
			}
			catch (System.Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send Eof message to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedEofMessageTransmission(message.Id, recipient.Id);
				canContinue = result.WasSuccessful;
			}
			return canContinue;
		}

		public async Task<bool> SendFileChunk(FileChunk fileChunk, Recipient recipient)
		{
			var canContinue = true;
			try
			{
				WorkerLog.Instance.Information($"Sending chunk '{fileChunk.SequenceNo}' to '{recipient.Name}'");
				await _transmissionLink.SendFileChunk(recipient, fileChunk);
				await _transmissionRepository.AddTransmissionResult(fileChunk.FileId, recipient.Id, fileChunk.SequenceNo, FileSource.Local);

			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Failed to send chunk to '{recipient.Name}'. Adding file to failed transmissions and retrying later");
				var result = await _transmissionRepository.AddFailedFileChunkTransmission(fileChunk.Id, fileChunk.FileId, recipient.Id, fileChunk.SequenceNo, fileChunk.GetValue());
				canContinue = result.WasSuccessful;
			}
			return canContinue;
		}

	}
}
