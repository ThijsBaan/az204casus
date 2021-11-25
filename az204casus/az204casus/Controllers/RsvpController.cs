using az204casus.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

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
        public IActionResult Submit(CreateRsvpModel model, IFormFile foto)
        {
            _logger.LogInformation("Received RSVP for {voornaam} {achternaam}", model.Voornaam, model.Achternaam);

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net";

            QueueClient queueClient = new QueueClient(connectionString, "reservations");
            queueClient.CreateIfNotExists();

            var naam = $"{model.Voornaam};{model.Achternaam}";

            BlobServiceClient blobClient = new BlobServiceClient(connectionString);
            var container = blobClient.GetBlobContainerClient("foto");
            container.CreateIfNotExists();


            container.UploadBlob($"{naam}.jpg", foto.OpenReadStream());

            queueClient.SendMessage(Base64Encode(naam));

            return View();
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

}
