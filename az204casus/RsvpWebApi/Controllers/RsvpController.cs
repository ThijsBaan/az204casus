using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
        public void Post([FromForm] RsvpModel rsvp)
        {
            _logger.LogInformation("Received RSVP for {voornaam} {achternaam}", rsvp.Voornaam, rsvp.Achternaam);

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net";

            var naam = $"{rsvp.Voornaam};{rsvp.Achternaam}";

            InsertReservation(rsvp, naam);
            UploadImage(rsvp, connectionString, naam);
        }

        private static void InsertReservation(RsvpModel rsvp, string naam)
        {
            var db = new MongoService().GetClient().GetDatabase("maxenthijsmongo");
            IMongoCollection<Reservation> reservations = db.GetCollection<Reservation>("Reservation");

            string id = Guid.NewGuid().ToString();
            reservations.InsertOne(new Reservation()
            {
                _id = id,
                Firstname = rsvp.Voornaam,
                Lastname = rsvp.Achternaam,
                PhotoUrl = $"{naam}.jpg",
                ThumbUrl = $"{naam}.jpg"
            });

            EventGridPublisherClient eventGridPublisherClient = CreateEventGridPublisherClient();
            eventGridPublisherClient.SendEvent(
                new EventGridEvent(
                    "ReservationCreatedEvent",
                    "FunctionApp.ReservationCreatedEvent",
                    "1.0",
                    JsonConvert.SerializeObject(new ReservationCreatedEvent() { ReserverationId = id })
                    )
                );
        }

        private static void UploadImage(RsvpModel rsvp, string connectionString, string naam)
        {
            BlobServiceClient blobClient = new BlobServiceClient(connectionString);
            var container = blobClient.GetBlobContainerClient("foto");
            container.CreateIfNotExists();

            string path = $"{naam}.jpg";

            container.UploadBlob(path, rsvp.foto.OpenReadStream());

            EventGridPublisherClient eventGridPublisherClient = CreateEventGridPublisherClient();
            eventGridPublisherClient.SendEvent(
                new EventGridEvent(
                    "ImageCreatedEvent",
                    "FunctionApp.ImageCreatedEvent",
                    "1.0",
                    JsonConvert.SerializeObject(new ImageCreatedEvent() { Path = path })
                    )
                );
        }

        private static EventGridPublisherClient CreateEventGridPublisherClient()
        {
            return new EventGridPublisherClient(new Uri("https://rsvpeventgridtopic.westeurope-1.eventgrid.azure.net/api/events"), new AzureKeyCredential("FQVGeyvZMJdiOIVohtNYKtuNsga672u4ViOT2ry4nj0="));
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
        public string _id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhotoUrl { get; set; }
        public string ThumbUrl { get; set; }
    }


    public class ImageCreatedEvent
    {
        public string Path { get; set; }
    }

    public class ReservationCreatedEvent
    {
        public string ReserverationId { get; set; }
    }

}
