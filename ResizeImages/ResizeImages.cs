using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ResizeImages
{
    public static class ResizeImages
    {
        private static readonly string BlobStorageConnectionString =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        
        private static readonly string OutputContainer = Environment.GetEnvironmentVariable("OUTPUT_CONTAINER");

        private const int ThumbnailWith = 200;

        [FunctionName("resize-images")]
        public static async Task RunAsync([BlobTrigger("images-to-resize/{name}.{extension}")]
            Stream myBlob, string name, string extension, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var encoder = GetEncoder(extension);
            if (encoder == null)
            {
                log.LogInformation($"No encoder found for: {extension}");
                return;
            }

            await using var output = new MemoryStream();
            if (!(await Image.LoadAsync(myBlob) is Image<Rgba32> image))
            {
                log.LogInformation($"No image has been loaded: {name}.{extension}");
                return;
            }

            try
            {
                var divisor = image.Width / ThumbnailWith;
                var height = Convert.ToInt32(Math.Round((decimal) image.Height / divisor));
                image.Mutate(x => x.Resize(ThumbnailWith, height));
                await image.SaveAsync(output, encoder);
                output.Seek(0, SeekOrigin.Begin);

                var blobServiceClient =
                    new BlobServiceClient(BlobStorageConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(OutputContainer);
                var blobClient = blobContainerClient.GetBlobClient($"{name}-{ThumbnailWith}.{extension}");

                await blobClient.UploadAsync(output, new BlobHttpHeaders()
                {
                    ContentType = GetContentType(extension)
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        private static IImageEncoder GetEncoder(string extension)
        {
            extension = extension.Replace(".", "");
            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);

            if (!isSupported) return null;

            IImageEncoder encoder = extension switch
            {
                "png" => new PngEncoder(),
                "jpg" => new JpegEncoder(),
                "jpeg" => new JpegEncoder(),
                "gif" => new GifEncoder(),
                _ => null
            };

            return encoder;
        }

        private static string GetContentType(string extension)
        {
            return extension switch
            {
                "png" => "image/png",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                _ => ""
            };
        }
    }
}