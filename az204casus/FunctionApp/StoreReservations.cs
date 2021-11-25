using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;

namespace FunctionApp
{
    public static class StoreReservations
    {
        [FunctionName("StoreReserverations")]
        public static void Run(
            [QueueTrigger("reservations", Connection = "STORAGE_CONNECTIONS_STRING")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var value = myQueueItem;

            log.LogInformation($"Processed message: {value}");


            var db = new MongoService().GetClient().GetDatabase("maxenthijsmongo");
            IMongoCollection<Reservation> reservations = db.GetCollection<Reservation>("Reservation");

            reservations.InsertOne(new Reservation()
            {
                _id = Guid.NewGuid().ToString(),
                Firstname = value.Split(";")[0],
                Lastname = value.Split(";")[1],
                PhotoUrl = $"{value}.jpg",
                ThumbUrl = $"{value}_th.jpg"
            });
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

    public class Reservation
    {
        public string _id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhotoUrl { get; set; }
        public string ThumbUrl { get; set; }
    }
}
