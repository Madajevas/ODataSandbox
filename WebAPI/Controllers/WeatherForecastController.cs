using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [EnableQuery]
        [HttpGet]
        [Authorize(Policy = "test")]
        public async IAsyncEnumerable<WeatherForecast> Get()
        {
            await Task.Yield(); // force async

            foreach (var index in Enumerable.Range(1, 5))
            {
                yield return new WeatherForecast
                {
                    Id = index,
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };
            }
        }
    }
}