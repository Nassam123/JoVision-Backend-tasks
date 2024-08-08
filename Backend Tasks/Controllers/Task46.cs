using Microsoft.AspNetCore.Mvc;
using System;

namespace Task46.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class BirthDateController : ControllerBase
    {
        [HttpPost("Task46")]
        public IActionResult Get([FromBody] Person person)
        {

            if (person.year == null || person.month == null || person.day == null)
            {
                return Ok($"Hello {person.name}, I can’t calculate your age without knowing your birthdate!");
            }

            try
            {
                DateTime birthdate = new DateTime(person.year.Value, person.month.Value, person.day.Value);
                int age = calculateAge(birthdate);

                return Ok($"Hello {person.name}, your age is {age}");
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
        
    }
    public class Person
    {
        public string name { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }

    }
    public class GreeterController:ControllerBase
            {
        [HttpPost("Task46")]
        public IActionResult Get([FromBody] string name = "Anonymous")
        {
            return Ok($"Hello {name}");
        }
         }
}
