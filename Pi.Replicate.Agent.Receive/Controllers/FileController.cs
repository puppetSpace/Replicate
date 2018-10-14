using Microsoft.AspNetCore.Mvc;
using System;

namespace Pi.Replicate.Agent.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpGet("received")]
        public IActionResult Received(Guid fileid)
        {
            return Ok(fileid);
        }

        [HttpGet("resend")]
        public IActionResult Resend(Guid fileid)
        {
            return Ok("resend");
        }
    }
}
