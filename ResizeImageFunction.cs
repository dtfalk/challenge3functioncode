// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Azure.Storage.Blobs;
// using Azure.Storage.Blobs.Models;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.Processing;
// using SixLabors.ImageSharp.Formats;
// using SixLabors.ImageSharp.Formats.Jpeg;
// using SixLabors.ImageSharp.Formats.Png;
// using SixLabors.ImageSharp.Formats.Gif;
// using SixLabors.ImageSharp.Formats.Bmp;

// namespace MyFunctionApp {
//     public class ResizeImageFunction
//     {
//         private readonly ILogger<ResizeImageFunction> _logger;
//         private readonly BlobServiceClient _blobServiceClient;

//         public ResizeImageFunction(ILogger<ResizeImageFunction> logger, BlobServiceClient blobServiceClient)
//         {
//             _logger = logger;
//             _blobServiceClient = blobServiceClient;
//         }

//         [Function("ResizeImageFunction")]
//         public async Task RunAsync(
//             [BlobTrigger("product-images/{name}", Connection = "DefaultEndpointsProtocol=https;AccountName=testingstorageaccount204;AccountKey=RWz04tx3H/SW1zEvCaqmqJh1nc6ME7O3cvY7Cm5DbVNSP5xUQbKtz961M97oE2mOOLQDiQAFk/o1+AStdsH+ag==;EndpointSuffix=core.windows.net")] Stream inputStream, 
//             string name)
//         {
//             _logger.LogInformation($"Processing new image: {name}");

//             try
//             {
//                 // Copy the incoming stream to a MemoryStream
//                 using var memoryStream = new MemoryStream();
//                 await inputStream.CopyToAsync(memoryStream);
//                 memoryStream.Position = 0;

//                 // Detect image format using ImageSharp
//                 IImageFormat detectedFormat = Image.DetectFormat(memoryStream);
//                 if (detectedFormat == null)
//                 {
//                     throw new InvalidOperationException("Unsupported image format.");
//                 }
//                 memoryStream.Position = 0;
//                 using var image = await Image.LoadAsync(memoryStream);

//                 // Resize the image to 100x100 pixels using Pad mode
//                 image.Mutate(x => x.Resize(new ResizeOptions
//                 {
//                     Size = new Size(100, 100),
//                     Mode = ResizeMode.Pad
//                 }));

//                 // Generate new file name with _thumb suffix
//                 var newFileName = $"{Path.GetFileNameWithoutExtension(name)}_thumb{Path.GetExtension(name)}";

//                 // Choose the appropriate encoder based on file extension
//                 IImageEncoder encoder = name.ToLower() switch
//                 {
//                     var ext when ext.EndsWith(".jpg") || ext.EndsWith(".jpeg") => new JpegEncoder(),
//                     var ext when ext.EndsWith(".bmp") => new BmpEncoder(),
//                     var ext when ext.EndsWith(".gif") => new GifEncoder(),
//                     _ => new PngEncoder(),
//                 };

//                 // Save the resized image into an output MemoryStream
//                 using var outputStream = new MemoryStream();
//                 await image.SaveAsync(outputStream, encoder);
//                 outputStream.Position = 0;

//                 // Use the injected BlobServiceClient to get the "resized-product-images" container
//                 var container = _blobServiceClient.GetBlobContainerClient("resized-product-images");
//                 await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
//                 var blobClient = container.GetBlobClient(newFileName);

//                 // Delete any existing blob to simulate "overwrite" behavior.
//                 await blobClient.DeleteIfExistsAsync();

//                 // Create BlobUploadOptions to include HTTP headers
//                 var uploadOptions = new BlobUploadOptions
//                 {
//                     HttpHeaders = new BlobHttpHeaders { ContentType = detectedFormat.DefaultMimeType }
//                 };

//                 // Upload the processed image
//                 await blobClient.UploadAsync(outputStream, uploadOptions);

//                 _logger.LogInformation($"Image resized and saved as: {newFileName}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error processing image {name}: {ex.Message}");
//             }
//         }
//     }
// }

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MyMinimalFunctionApp
{
    public class HelloWorldFunction
    {
        private readonly ILogger _logger;

        public HelloWorldFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HelloWorldFunction>();
        }

        [Function("HelloWorldFunction")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("HelloWorldFunction triggered.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("Hello, world!");
            return response;
        }
    }
}