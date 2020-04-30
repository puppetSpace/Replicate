using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Models;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{


		public FileController()
		{
		}

		[HttpPost]
		public IActionResult Post([FromBody] FileTransmissionModel model)
		{
			return Ok();
		}
	}
}
