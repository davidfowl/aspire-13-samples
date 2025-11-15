#:package Aspire.Hosting.Python@13.0.0
#:sdk Aspire.AppHost.Sdk@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonApp("script", "./script", "main.py");

builder.Build().Run();
