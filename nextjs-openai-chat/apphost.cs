#:package Aspire.Hosting.JavaScript@13.0.0
#:package Aspire.Hosting.OpenAI@13.0.0
#:package Aspire.Hosting.Docker@13-*

#:sdk Aspire.AppHost.Sdk@13.0.0

using Aspire.Hosting.ApplicationModel.Docker;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

// OpenAI API key - user provides this
var openAi = builder.AddOpenAI("openai");

// Next.js chat app with OpenAI streaming
var chat = builder.AddNodeApp("chat", "./chat", "server.js")
                  .WithRunScript("dev")
                  .WithBuildScript("build")
                  .WithEnvironment("OPENAI_API_KEY", openAi.Resource.Key)
                  .WithHttpEndpoint(port: 3000, env: "PORT")
                  .WithExternalHttpEndpoints()
                  .PublishAsDockerFile(c =>
                  {
#pragma warning disable ASPIREDOCKERFILEBUILDER001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                      c.WithDockerfileBuilder("./chat", builder =>
                     {
                         var buildStage = builder.Builder.Stages[1];

                         // Mutate the copy statement
                         //buildStage.Statements.Where()

                         var copyStatements = buildStage.Statements.Where(s => s.GetType().Name == "DockerfileCopyStatement");
                     });
#pragma warning restore ASPIREDOCKERFILEBUILDER001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                  });

builder.Build().Run();
