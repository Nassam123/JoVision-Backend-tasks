using Microsoft.AspNetCore.Mvc;
using System;

namespace Task45.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class BirthDateController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string name = "Anonymous", [FromQuery] int? year = null, [FromQuery] int? month = null, [FromQuery] int? day = null)
        {
            if (year == null || month == null || day == null)
            {
                return Ok($"Hello {name}, I can’t calculate your age without knowing your birthdate!");
            }

            try
            {
                DateTime birthdate = new DateTime(year.Value, month.Value, day.Value);
                int age = calculateAge(birthdate);

                return Ok($"Hello {name}, your age is {age}");
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest("Invalid birthdate parameters provided.");
            }
        }

        int calculateAge(DateTime birthdate)
        {
            int age = DateTime.Today.Year - birthdate.Year;
            if (DateTime.Today.DayOfYear < birthdate.DayOfYear)
            {
                age--;
            }
            return age;
        }


        [HttpGet("Greeter")]
        public IActionResult Get([FromQuery] string name = "Anonymous")
        {
            return Ok($"Hello {name}");
        }
    }
}
