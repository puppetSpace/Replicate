using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Processing;
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
        private readonly IWorkItemQueue<FileChunk> _workItemQueue;

        public FileChunkController(IWorkItemQueueFactory IWorkItemQueueFactory)
        {
            _workItemQueue = IWorkItemQueueFactory.GetQueue<FileChunk>(QueueKind.Incoming);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FileChunk fileChunk)
        {
            //todo checks
            await _workItemQueue.Enqueue(fileChunk);
            return Ok();
        }
    }
}
