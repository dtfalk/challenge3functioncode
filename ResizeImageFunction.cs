using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkiaSharp;

public class ResizeImageFunction
{
    private readonly ILogger<ResizeImageFunction> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public ResizeImageFunction(ILogger<ResizeImageFunction> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("ResizeImageFunction")]
    public async Task RunAsync(
        [BlobTrigger("product-images/{name}", Connection = "AzureWebJobsStorage")] Stream imageStream, 
        string name)
    {
        _logger.LogInformation($"Processing new image: {name}");

        try
        {
            // Read the original image
            using var inputStream = new MemoryStream();
            await imageStream.CopyToAsync(inputStream);
            inputStream.Position = 0;
            using var originalImage = SKBitmap.Decode(inputStream);

            // Resize the image to 200x200 pixels
            using var resizedImage = originalImage.Resize(new SKImageInfo(200, 200), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedImage);
            using var outputStream = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(outputStream);
            outputStream.Position = 0;

            // Save resized image to another blob container
            var outputContainer = _blobServiceClient.GetBlobContainerClient("resized-product-images");
            await outputContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var outputBlob = outputContainer.GetBlobClient(name);

            await outputBlob.UploadAsync(outputStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

            _logger.LogInformation($"Image resized and saved: {name}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing image {name}: {ex.Message}");
        }
    }
}
