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
    public class EofMessageController : ControllerBase
    {
        
		[HttpPost("api/file/{fileId}/eot")]
		public IActionResult Post([FromQuery]Guid fileId, [FromBody] EofMessageTransmissionModel model)
		{

			return Ok();
		}

    }
}
