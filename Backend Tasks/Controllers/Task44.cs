using Microsoft.AspNetCore.Mvc;

namespace Backend_Tasks.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class Greeter : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string name = "Anonymous")
        {
            return Ok($"Hello {name}");
        }
    }
}