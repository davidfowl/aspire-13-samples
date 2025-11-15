#:package Aspire.Hosting.OpenAI@13.0.0
#:package Aspire.Hosting.Python@13.0.0
#:package Aspire.Hosting.Docker@13-*
#:sdk Aspire.AppHost.Sdk@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

// Add OpenAI connection (configured via user secrets or environment variables)
var openai = builder.AddOpenAI("openai");

// Add Python AI agent using FastAPI
builder.AddUvicornApp("ai-agent", "./agent", "main:app")
    .WithUv()
    .WithExternalHttpEndpoints()
    .WithEnvironment("OPENAI_API_KEY", openai.Resource.Key);

builder.Build().Run();
