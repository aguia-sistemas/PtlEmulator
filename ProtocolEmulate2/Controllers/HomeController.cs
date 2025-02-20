// HomeController.cs
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ProtocolEmulate2.Models;
using ProtocolEmulate2.Services;

namespace ProtocolEmulate2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DisplayService _displayService;

        public HomeController(ILogger<HomeController> logger, DisplayService displayService)
        {
            _logger = logger;
            _displayService = displayService;
        }

        public IActionResult Index()
        {
            return View(_displayService.ClientDisplays);
        }

        [HttpGet]
        public JsonResult GetDisplayData()
        {
            return Json(_displayService.ClientDisplays);
        }

        [HttpPost]
        public IActionResult SendConfirm([FromBody] ComfirmRequest request)
        {
            if (request == null || request.ClientId == 0 || request.DisplayId == 0 || request.Value == null)
            {
                return BadRequest("Invalid request data.");
            }

            _displayService.SendMessageToClient(request.ClientId, "Confirm", request.DisplayId.ToString(), request.Value.ToString());
            return Json(new { success = true });
        }
        
        
        [HttpPost]
        public async Task<IActionResult> ProcessScanner([FromBody] ScannerRequest scannerRequest)
        {
            var client = new HttpClient();
            var requestUri = $"http://localhost:15672/api/exchanges/%2f/{scannerRequest.ScannerEst}/publish";
            var body = $"P01,{scannerRequest.ScannerEst},{scannerRequest.ScannerValue},{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}";
            var jsonContent = $"{{\"properties\":{{}},\"routing_key\":\"{scannerRequest.ScannerEst}\",\"payload\":\"{{\\\"body\\\":\\\"{body}\\\"}}\",\"payload_encoding\":\"string\"}}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest")));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessProtocol([FromBody] ProtocolRequest protocolRequest)
        {
            var client = new HttpClient();
            var requestUri = $"http://localhost:15672/api/exchanges/%2f/{protocolRequest.ScannerEst}/publish";
            var body = $"{protocolRequest.Protocol},{protocolRequest.ScannerEst},{protocolRequest.ScannerValue},{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}";
            var jsonContent = $"{{\"properties\":{{}},\"routing_key\":\"{protocolRequest.Protocol}\",\"payload\":\"{{\\\"body\\\":\\\"{body}\\\"}}\",\"payload_encoding\":\"string\"}}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest")));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return Json(new { success = true });
        }
    }
}