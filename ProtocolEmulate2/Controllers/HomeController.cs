// HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtocolEmulate2.Models;
using ProtocolEmulate2.Services;
using PtlEmulator.App.Command;

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
            return View(_displayService.DisplayDictionary);
        }

        [HttpGet]
        public JsonResult GetDisplayData()
        {
            return Json(_displayService.DisplayDictionary);
        }
        
        [HttpPost]
        public IActionResult SendConfirm([FromBody] ComfirmRequest request)
        {
            _displayService.SendMessageToClient(request.ClientId, "Confirm", request.DisplayId.ToString(), request.Value.ToString());
            return Json(new { success = true });
        }
    }
}