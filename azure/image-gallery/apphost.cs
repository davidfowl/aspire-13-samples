#pragma warning disable ASPIRECSHARPAPPS001
#pragma warning disable ASPIREAZURE002

#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.Azure.Storage@13.0.0
#:package Aspire.Hosting.Azure.Sql@13.0.0
#:package Aspire.Hosting.JavaScript@13.0.0
#:package Aspire.Hosting.Azure.AppContainers@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("env");

// Storage: Use Azurite emulator in run mode, real Azure in publish mode
var storage = builder.AddAzureStorage("storage")
                .RunAsEmulator();

var blobs = storage.AddBlobContainer("images");
var queues = storage.AddQueues("queues");

// Azure SQL Database
var sql = builder.AddAzureSqlServer("sql")
    .RunAsContainer()
    .AddDatabase("imagedb");

// API: Upload images, queue thumbnail jobs, serve metadata
var api = builder.AddCSharpApp("api", "./api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WaitFor(sql)
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(sql)
    .WithUrls(context =>
    {
        foreach (var u in context.Urls)
        {
            u.DisplayLocation = UrlDisplayLocation.DetailsOnly;
        }

        context.Urls.Add(new()
        {
            Url = "/scalar",
            DisplayText = "API Reference",
            Endpoint = context.GetEndpoint("https")
        });
    })
    .PublishAsAzureContainerApp((infra, app) =>
    {
        // Scale to zero when idle
        app.Template.Scale.MinReplicas = 0;
    });

// Worker: Container Apps Job for queue-triggered thumbnail generation
// Runs every 2 minutes, polls for up to 2 minutes or until queue is empty
var worker = builder.AddCSharpApp("worker", "./worker")
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(sql)
    .WaitFor(sql)
    .WaitFor(queues)
    .PublishAsScheduledAzureContainerAppJob("*/2 * * * *");

// Frontend: Vite+React for upload and gallery UI
var frontend = builder.AddViteApp("frontend", "./frontend")
    .WithEndpoint("http", e => e.Port = 9080)
    .WithReference(api)
    .WithUrl("", "Image Gallery");

// Publish: Embed frontend build output in API container
api.PublishWithContainerFiles(frontend, "wwwroot");

builder.Build().Run();
