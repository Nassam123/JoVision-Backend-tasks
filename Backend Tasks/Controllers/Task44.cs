using Microsoft.AspNetCore.Mvc;

namespace Task44.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class GreeterController : ControllerBase
    {
        [HttpGet("Task44")]
        public IActionResult Get([FromQuery] string name = "Anonymous")
        {
            return Ok($"Hello {name}");
        }
    }
}