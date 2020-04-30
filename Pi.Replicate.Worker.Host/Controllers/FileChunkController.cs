using Microsoft.AspNetCore.Mvc;
using Pi.Replicate.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
    public class FileChunkController : ControllerBase
    {
        
		[HttpPost("api/file/{fileId}/chunk")]
		public IActionResult Post([FromQuery] Guid fileId, [FromBody] FileChunkTransmissionModel model)
		{
			return Ok();
		}
    }
}
