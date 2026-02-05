using Microsoft.AspNetCore.Mvc;
using WeatherForecastService.Models;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IForecastService service, ICacheService cacheService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(string city, string country, DateTime day)
        {
            if (!ValidateRequest(city, country, day))
            {
                return BadRequest("One ore more arguments are incorrect.");
            }

            var result = await service.GetWeatherForecastAsync(
                city,
                country,
                DateOnly.FromDateTime(day),
                HttpContext.RequestAborted);

            return Ok(result);
        }

        private bool ValidateRequest(string city, string country, DateTime day)
            => !string.IsNullOrWhiteSpace(city)
               && !(string.IsNullOrWhiteSpace(country))
               && !(day > DateTime.UtcNow.AddDays(10))
               && !(day < DateTime.UtcNow);        
    }
}
