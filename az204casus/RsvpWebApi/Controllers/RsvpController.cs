using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace RsvpWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RsvpController : ControllerBase
    {

        private readonly ILogger<RsvpController> _logger;

        public RsvpController(ILogger<RsvpController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Reservation> Get()
        {
            var db = new MongoService().GetClient().GetDatabase("maxenthijsmongo");
            IMongoCollection<Reservation> reservations = db.GetCollection<Reservation>("Reservation");

            return reservations.Find(_ => true).ToList();
        }

        [HttpPost]
        public void Post([FromBody] RsvpModel rsvp)
        {
            _logger.LogInformation("Received RSVP for {voornaam} {achternaam}", rsvp.Voornaam, rsvp.Achternaam);

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net";

            QueueClient queueClient = new QueueClient(connectionString, "reservations");
            queueClient.CreateIfNotExists();

            var naam = $"{rsvp.Voornaam};{rsvp.Achternaam}";

            BlobServiceClient blobClient = new BlobServiceClient(connectionString);
            var container = blobClient.GetBlobContainerClient("foto");
            container.CreateIfNotExists();

            container.UploadBlob($"{naam}.jpg", rsvp.foto.OpenReadStream());

            queueClient.SendMessage(Base64Encode(naam));
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    public class RsvpModel
    {
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public IFormFile foto { get; set; }

    }


    public class MongoService
    {
        private static MongoClient _client;
        public MongoService()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://maxenthijsmongo:XMbqjIVM7FurSW1IuFNZpqHZyyoaFqJHRo0s6PaoKCyHXnTNKA8pxDJDcGptoz9rGxoLEDJlPXZFMBFJDuE0zg==@maxenthijsmongo.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@maxenthijsmongo@"));
            _client = new MongoClient(settings);
        }

        public MongoClient GetClient()
        {
            return _client;
        }
    }

    public class Reservation
    {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhotoUrl { get; set; }
        public string ThumbUrl { get; set; }
    }
}
