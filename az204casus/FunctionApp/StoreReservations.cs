using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;

namespace FunctionApp
{
    public static class StoreReservations
    {
        [FunctionName("StoreReserverations")]
        public static void Run(
            [EventGridTrigger] EventGridEvent e,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed.");

            log.LogInformation(e.Data.ToJson());

            var value = e.Data.ToObjectFromJson<ReservationCreatedEvent>();

            log.LogInformation($"Processed event: {value}");

            var db = new MongoService().GetClient().GetDatabase("maxenthijsmongo");
            IMongoCollection<Reservation> reservations = db.GetCollection<Reservation>("Reservation");

            var filter = Builders<Reservation>.Filter.Eq("_id", value.ReserverationId);
            var update = Builders<Reservation>.Update.Set("Created", DateTime.UtcNow);
            reservations.UpdateOneAsync(filter, update);
        }
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

    public class ReservationCreatedEvent
    {
        public string ReserverationId { get; set; }
    }

    public class Reservation
    {
        public string _id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhotoUrl { get; set; }
        public string ThumbUrl { get; set; }
    }
}
