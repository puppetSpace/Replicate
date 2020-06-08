using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
    public class ProbeController : ControllerBase
    {
        public IActionResult Get()
		{
			WorkerLog.Instance.Information($"Someone is trying to verify this point");
			return Ok(System.Text.Json.JsonSerializer.Serialize(Environment.MachineName));
		}
    }
}
