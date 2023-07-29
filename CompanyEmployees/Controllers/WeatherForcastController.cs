using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForcastController : ControllerBase
    {
        private ILoggerManager _logger;

        public WeatherForcastController(ILoggerManager logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInfo("Log info message.");
            _logger.LogDebug("Log debug message.");

            return Ok();
        }
    }
}
