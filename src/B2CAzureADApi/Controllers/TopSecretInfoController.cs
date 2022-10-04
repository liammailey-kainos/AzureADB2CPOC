using B2CAzureADApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace B2CAzureADApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureADB2C:Scope")]
    public class TopSecretInfoController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<TopSecretInfoController> _logger;

        public TopSecretInfoController(ILogger<TopSecretInfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetTopSecretInfo")]
        public IEnumerable<TopSecretInfo> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new TopSecretInfo
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}