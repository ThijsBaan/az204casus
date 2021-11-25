using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace WebJob
{

    public class Reservation : ITableEntity
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string photoUrl { get; set; }
        public string thumbUrl { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp{get; set;}
        public Azure.ETag ETag
        {
            get; set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net";

            QueueClient queueClient = new QueueClient(connectionString, "reservations");
            queueClient.CreateIfNotExists();

            var message = queueClient.ReceiveMessage();
            if (message == null || message.Value == null)
            {
                Console.WriteLine("No message found :(!");
                return;
            }

            var value = message.Value;

            DecreaseImage(connectionString, value);


            Console.WriteLine($"Processed message: {value.Body}");

            TableClient tableClient = new TableClient(connectionString, "reservations");
            tableClient.CreateIfNotExists();

            tableClient.AddEntity<Reservation>(new Reservation()
            {
                Firstname = value.MessageText.Split(";")[0],
                Lastname = value.MessageText.Split(";")[1],
                photoUrl = $"{value.Body}.jpg",
                thumbUrl = $"{value.Body}_th.jpg",
                PartitionKey = value.MessageText.Split(";")[1][0].ToString(),
                RowKey = value.MessageText,
                ETag = ETag.All
            });

            queueClient.DeleteMessage(value.MessageId, value.PopReceipt);
        }

        private static void DecreaseImage(string connectionString, Azure.Storage.Queues.Models.QueueMessage value)
        {
            BlobServiceClient bsc = new BlobServiceClient(connectionString);
            var container = bsc.GetBlobContainerClient("foto");
            container.CreateIfNotExists();

            BlobClient bc = new BlobClient(connectionString, "foto", $"{value.Body}.jpg");

            var stream = new MemoryStream();
            using (Image image = Image.Load(bc.OpenRead()))
            {
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));

                image.Save(stream, new PngEncoder());
            }

            stream.Position = 0;
            container.UploadBlob($"{value.Body}_th.jpg", stream);
        }
    }
}
