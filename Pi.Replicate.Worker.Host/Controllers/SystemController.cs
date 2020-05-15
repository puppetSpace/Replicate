﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    public class SystemController : ControllerBase
    {
        public async Task<ActionResult<SystemOverview>> Get()
		{
			var systemService = new SystemService();
			return await systemService.GetSystemOverview();
		}
    }
}
