using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;
		private readonly IFileChunkRepository _fileChunkRepository;
		private readonly IWorkItemQueue<FileChunk> _workItemQueue;

        public FileController(IFileRepository fileRepository, IFileChunkRepository fileChunkRepository, IWorkItemQueueFactory workItemQueueFactory)
        {
            _fileRepository = fileRepository;
			_fileChunkRepository = fileChunkRepository;
			_workItemQueue = workItemQueueFactory.GetQueue<FileChunk>(QueueKind.Outgoing);
        }

        [HttpGet("received")]
        public async Task<IActionResult> Received(Guid fileId)
        {
            var file = await _fileRepository.Get(fileId);
            if (file != null)
            {
                file.Status = FileStatus.UploadSucessful;
                await _fileRepository.Update(file);
            }

            return Ok();
        }

        [HttpGet("resend")]
        public async Task<IActionResult> Resend(Guid fileId)
        {
			//todo get host
			var fileChunks = await _fileChunkRepository.GetForFile(fileId);
			foreach(var fileChunk in fileChunks)
			{
				var hostFileChunk = new HostFileChunk(fileChunk) { Host = null };
				await _workItemQueue.Enqueue(hostFileChunk);
			}
            return Ok();
        }
    }
}
