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

        [FunctionName("resize-images")]
        public static async Task RunAsync([BlobTrigger("images-to-resize/{name}.{extension}")]
            Stream myBlob, string name, string extension, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            await ResizeImage(myBlob, name, extension, log, 200);
        }

        #region extra functions

        [FunctionName("resize-images-400")]
        public static async Task RunAsyncMedium([BlobTrigger("images-to-resize/{name}.{extension}")]
            Stream myBlob, string name, string extension, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        
            await ResizeImage(myBlob, name, extension, log, 400);
        }
        
        [FunctionName("resize-images-600")]
        public static async Task RunAsyncLarge([BlobTrigger("images-to-resize/{name}.{extension}")]
            Stream myBlob, string name, string extension, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        
            await ResizeImage(myBlob, name, extension, log, 600);
        }

        #endregion

        private static async Task ResizeImage(Stream myBlob, string name, string extension, ILogger log,
            int thumbnailWidth)
        {
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
                var divisor = image.Width / thumbnailWidth;
                var height = Convert.ToInt32(Math.Round((decimal) image.Height / divisor));
                image.Mutate(x => x.Resize(thumbnailWidth, height));
                await image.SaveAsync(output, encoder);
                output.Seek(0, SeekOrigin.Begin);

                var blobServiceClient =
                    new BlobServiceClient(BlobStorageConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(OutputContainer);
                var blobClient = blobContainerClient.GetBlobClient($"{name}-{thumbnailWidth}.{extension}");

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