using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("This endpoint is public");
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("You are an Administrator");
        }

        [Authorize(Roles = "Nurse")]
        [HttpGet("nurse-only")]
        public IActionResult NurseOnly()
        {
            return Ok("You are a Nurse");

        }

    }
}