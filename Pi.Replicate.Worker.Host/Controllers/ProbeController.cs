using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[ApiController]
	[Route("[controller]")]
    public class ProbeController : ControllerBase
    {
        public IActionResult Get()
		{
			return Ok();
		}
    }
}
