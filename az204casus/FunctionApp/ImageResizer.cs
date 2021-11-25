using System;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace FunctionApp
{
    public static class ImageResizer
    {
        [FunctionName("ImageResizer")]
        public static void Run([BlobTrigger("foto/{name}", Connection = "STORAGE_CONNECTIONS_STRING")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=rsvpstorageaccount;AccountKey=JyZyWNsarrgCVX2UZ/gbNW842/4bB438WyAzkUjaijPY3KzbRxz2+I9fL+DzG0eILh1UtIEn1v8ZKNeQyV07Qg==;EndpointSuffix=core.windows.net";
            BlobServiceClient bsc = new BlobServiceClient(connectionString);
            var container = bsc.GetBlobContainerClient("thumb");
            container.CreateIfNotExists();

            var stream = new MemoryStream();
            using (Image image = Image.Load(myBlob))
            {
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));

                image.Save(stream, new PngEncoder());
            }

            stream.Position = 0;
            container.UploadBlob(name, stream);
        }
    }
}
