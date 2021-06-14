using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SerilogWapp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        readonly IDiagnosticContext _diagnosticContext;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiagnosticContext diagnosticContext)
        {
            _logger = logger;
            _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            IEnumerable<WeatherForecast> weatherForecastData =
                Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                });

            _diagnosticContext.Set("weatherForecastData temperature", "test");
            _logger.LogInformation("Got weather forecast data: {@WeatherForecast}", weatherForecastData);

            return weatherForecastData.ToArray();
        }
    }
}