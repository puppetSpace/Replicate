using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileChunkController : ControllerBase
    {
        private readonly IWorkItemQueue<FileChunk> _incomingWorkItemQueue;
		private readonly IWorkItemQueue<FileChunk> _outgoingWorkItemQueue;
		private readonly IFileChunkRepository _fileChunkRepository;

		public FileChunkController(IWorkItemQueueFactory IWorkItemQueueFactory, IFileChunkRepository fileChunkRepository)
        {
            _incomingWorkItemQueue = IWorkItemQueueFactory.GetQueue<FileChunk>(QueueKind.Incoming);
            _outgoingWorkItemQueue = IWorkItemQueueFactory.GetQueue<FileChunk>(QueueKind.Outgoing);
			_fileChunkRepository = fileChunkRepository;
		}

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FileChunk fileChunk)
        {
            //todo checks
            await _incomingWorkItemQueue.Enqueue(fileChunk);
            return Ok();
        }

		[HttpGet("resend")]
		public async Task<IActionResult> Resend(Guid fileChunkId)
		{
			//todo get host
			var fileChunk = await _fileChunkRepository.Get(fileChunkId);
			if(fileChunk != null)
			{
				var hostFileChunk = new HostFileChunk(fileChunk) { Host = null };
				await _incomingWorkItemQueue.Enqueue(hostFileChunk);
			}
			return Ok();
		}
	}
}
