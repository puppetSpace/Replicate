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
        private readonly IWorkItemQueue<File> _workItemQueue;

        public FileController(IFileRepository fileRepository, IWorkItemQueueFactory workItemQueueFactory)
        {
            _fileRepository = fileRepository;
            _workItemQueue = workItemQueueFactory.GetQueue<File>(QueueKind.Outgoing);
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
            //todo add chunks on the to send queue, do not process file again
            //todo only send to  endpoint that asked for resend
            //wrap filechunk in new class ResendFileChunk
            var file = await _fileRepository.Get(fileId);
            if (file != null)
                await _workItemQueue.Enqueue(file);
            return Ok();
        }
    }
}
