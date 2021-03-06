using System;
using System.IO;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using MongoDB.Bson;

namespace FunctionApp
{
    public static class ImageResizer
    {
        [FunctionName("ImageResizer")]
        public static void Run(
            [EventGridTrigger] EventGridEvent e, 
            ILogger log)
        {
            log.LogInformation(e.Data.ToJson());

            ImageCreatedEvent imageCreatedEvent = e.Data.ToObjectFromJson<ImageCreatedEvent>();

            
            var connectionString = Environment.GetEnvironmentVariable("storage-connectionstring", EnvironmentVariableTarget.Process);
            BlobServiceClient bsc = new BlobServiceClient(connectionString);
            var container = bsc.GetBlobContainerClient("thumb");
            container.CreateIfNotExists();


            BlobClient bc = new BlobClient(connectionString, "foto", imageCreatedEvent.Path);

            var stream = new MemoryStream();
            using (Image image = Image.Load(bc.OpenRead()))
            {
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));

                image.Save(stream, new PngEncoder());
            }

            stream.Position = 0;
            container.UploadBlob(imageCreatedEvent.Path, stream);
        }
    }

    public class ImageCreatedEvent
    {
        public string Path { get; set; }
    }

}
