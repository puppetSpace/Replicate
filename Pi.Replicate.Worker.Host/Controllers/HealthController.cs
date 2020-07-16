using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[Route("api/health")]
	[ApiController]
    public class HealthController : ControllerBase
    {
        public IActionResult Get()
		{
			return Ok("All good boss");
		}
    }
}
