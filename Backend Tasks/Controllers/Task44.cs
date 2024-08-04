using Microsoft.AspNetCore.Mvc;

namespace Task44.Controllers
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