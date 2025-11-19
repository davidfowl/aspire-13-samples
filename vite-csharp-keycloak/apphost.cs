#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.Keycloak@13-*
#:package Aspire.Hosting.Docker@13-*
#:package Aspire.Hosting.JavaScript@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

// Keycloak Identity Provider
var keycloak = builder.AddKeycloak("keycloak")
                      // .WithDataVolume() // Persist Keycloak data between runs
                      .WithRealmImport("./Realms") // Import realm configuration
                      .WithUrl("", "Keycloak Admin");

// BFF (Backend for Frontend) - serves frontend and handles auth
var bff = builder.AddCSharpApp("bff", "./bff")
                 .WithHttpHealthCheck("/health")
                 .WithExternalHttpEndpoints()
                 .WaitFor(keycloak)
                 .WithReference(keycloak)
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
                 });

// Vite Frontend - served by BFF
var frontend = builder.AddViteApp("frontend", "./frontend")
                      .WithEndpoint("http", e => e.Port = 9082) // Fixed port
                      .WithReference(bff)
                      .WithUrl("", "Keycloak Demo");

// When in development mode, configure BFF_URL to point at the vite frontend
if (builder.ExecutionContext.IsRunMode)
{
    keycloak.WithEnvironment("BFF_URL", frontend.GetEndpoint("http", KnownNetworkIdentifiers.LocalhostNetwork));
}
else
{
    // When in publish mode, configure BFF_URL to point at the BFF HTTPS endpoint
    // since the frontend is served from there
    keycloak.WithEnvironment("BFF_URL", bff.GetEndpoint("http"));
}

// Publish: Embed frontend build output in BFF container
bff.PublishWithContainerFiles(frontend, "wwwroot");

builder.Build().Run();
