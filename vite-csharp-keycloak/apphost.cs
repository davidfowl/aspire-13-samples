#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Hosting.Keycloak@13-*
#:package Aspire.Hosting.JavaScript@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

// Client secret for OAuth/OIDC
var generatedPassword = new GenerateParameterDefault
{
    MinLength = 22, // enough to give 128 bits of entropy when using the default 67 possible characters. See remarks in GenerateParameterDefault
};

var bffClientSecret = builder.AddParameter("bff-client-secret", generatedPassword, secret: true);

// Keycloak Identity Provider
var keycloak = builder.AddKeycloak("keycloak")
                      // .WithDataVolume() // Persist Keycloak data between runs
                      .WithRealmImport("./Realms") // Import realm configuration
                      .WithEnvironment("BFF_CLIENT_SECRET", bffClientSecret)
                      .WithUrl("", "Keycloak Admin");

// BFF (Backend for Frontend) - serves frontend and handles auth
var bff = builder.AddCSharpApp("bff", "./bff")
                 .WithHttpHealthCheck("/health")
                 .WithExternalHttpEndpoints()
                 .WithEnvironment("BFF_CLIENT_SECRET", bffClientSecret)
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
