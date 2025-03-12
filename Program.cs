using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Add Azure Blob Storage Client
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;
        services.AddSingleton(new BlobServiceClient(storageConnectionString));
    })
    .Build();

host.Run();
