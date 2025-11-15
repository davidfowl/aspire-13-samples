#:package Aspire.Hosting.Python@13.0.0
#:package Aspire.Hosting.PostgreSQL@13.0.0
#:package Aspire.Hosting.Docker@13-*
#:sdk Aspire.AppHost.Sdk@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin()
                      .AddDatabase("db");

builder.AddUvicornApp("api", "./api", "main:app")
       .WithExternalHttpEndpoints()
       .WaitFor(postgres)
       .WithReference(postgres);

builder.Build().Run();
