using az204casus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace az204casus.Controllers
{
    public class RsvpController : Controller
    {
        private readonly ILogger<RsvpController> _logger;

        public RsvpController(ILogger<RsvpController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Submit(CreateRsvpModel model)
        {
            _logger.LogInformation("Received RSVP for {voornaam} {achternaam}", model.Voornaam, model.Achternaam);

            return View();
        }
    }
}
