# Aspire Static Files Sample

**Vite based app + YARP for static file serving + Docker Compose for hosting.**

This sample demonstrates how to use YARP (Yet Another Reverse Proxy) with Aspire to serve static files from a Vite.js frontend application.

## Quick Start

### Prerequisites

- [Aspire CLI](https://aspire.dev/get-started/install-cli/)
- [Docker](https://docs.docker.com/get-docker/)

### Commands

**Run locally** (automatically runs `npm run dev` for you):

```bash
aspire run
```

**Deploy to Docker Compose**:

```bash
aspire deploy
```

**Teardown Docker Compose deployment**:

```bash
aspire do docker-compose-down-dc
```

**Build container images** (optional):

```bash
aspire do build
```

## Overview

The application consists of:

- **Aspire AppHost** - Orchestrates the application and configures YARP
- **YARP** - Serves static files from the Vite.js frontend
- **Vite.js Frontend** - A simple JavaScript application with a counter component

## Key Code

The AppHost configuration demonstrates how to use YARP with different behaviors for run mode vs publish mode:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

var frontend = builder.AddViteApp("frontend", "./frontend");

builder.AddYarp("app")
       .WithConfiguration(c =>
       {
           if (builder.ExecutionContext.IsRunMode)
           {
               // In run mode, forward all requests to vite dev server
               c.AddRoute("{**catch-all}", frontend);
           }
       })
       .WithExternalHttpEndpoints()
       .PublishWithStaticFiles(frontend);
```

Key features:

- **Docker Compose Environment**: Registers a Docker Compose environment for deployment
- **Development Mode**: Routes all requests to the Vite dev server for hot reload
- **Production Mode**: Serves static files from the built frontend
- **External HTTP Endpoints**: Enables external access to the YARP proxy

## Deployment

The sample uses Docker Compose for deployment. Simply run:

```bash
aspire deploy
```

This will:

1. Build the Vite frontend
2. Create container images
3. Generate Docker Compose files
4. Deploy the application

To teardown the deployment:

```bash
aspire do docker-compose-down-dc
```
