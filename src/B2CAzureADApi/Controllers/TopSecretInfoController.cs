using System.Net.Http.Headers;
using System.Text.Json;
using B2CAzureADApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        private readonly IHttpClientFactory _httpClientFactory;

        public TopSecretInfoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetTopSecretInfo")]
        public async Task<IActionResult> Get()
        {
            var results = new List<TopSecretInfo>();
            string? accessToken;
            try
            {
                accessToken = await HttpContext.GetTokenAsync("B2C", "access_token");
            }
            catch
            {
                accessToken = await HttpContext.GetTokenAsync("AAD", "access_token");
            }
            
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44312/WeatherForecast");

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await client.SendAsync(request);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.OK => Ok(
                    JsonSerializer.Deserialize<List<TopSecretInfo>>(await response.Content.ReadAsStringAsync())),
                System.Net.HttpStatusCode.Unauthorized => Unauthorized(),
                _ => Problem(detail: response.ToString(), statusCode: (int)response.StatusCode)
            };
        }
    }
}