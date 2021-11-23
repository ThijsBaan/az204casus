using az204casus.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

            QueueClient queueClient = new QueueClient("DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net", "reservations");
            queueClient.CreateIfNotExists();

            queueClient.SendMessage($"{model.Voornaam};{model.Achternaam}");
            return View();
        }
    }
}
