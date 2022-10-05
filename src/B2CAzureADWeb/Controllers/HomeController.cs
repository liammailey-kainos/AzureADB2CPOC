using B2CAzureADWeb.Extensions;
using B2CAzureADWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace B2CAzureADWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Restricted()
        {
            return View();
        }

        [Authorize(Roles="business")]
        public IActionResult Business()
        {
            return View();
        }

        [Authorize(Roles = "government")]
        public IActionResult Government()
        {
            return View();
        }

        public IActionResult Unauthorised()
        {
            return View();
        }

        public async Task<IActionResult> CallApi()
        {
            await MakeApiCall("https://localhost:44364/TopSecretInfo");
            ViewBag.ApiCall = "1";
            return View("CallApi");
        }
        
        public async Task<IActionResult> CallApi2()
        {
            await MakeApiCall("https://localhost:44312/WeatherForecast");
            ViewBag.ApiCall = "2";
            return View("CallApi");
        }

        private async Task MakeApiCall(string address)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, address);
            
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await client.SendAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = response.ToString();
            }
            else
            {
                ViewBag.IsSuccess = true;
                var jsonString = await response.Content.ReadAsStringAsync();
                ViewBag.Message = jsonString.JsonPrettify();
            }
        }
        
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}