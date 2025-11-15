# Node.js Express + Redis + Vite Frontend Sample

**Express API with Redis caching and React TypeScript frontend using YARP.**

This sample demonstrates Aspire 13's JavaScript/Node.js polyglot support with a visit counter application featuring an Express backend, Redis state management, and a modern Vite + React + TypeScript frontend.

## Quick Start

### Prerequisites

- [Aspire CLI](https://aspire.dev/get-started/install-cli/)
- [Docker](https://docs.docker.com/get-docker/)

### Commands

**Run locally** (automatically installs npm dependencies):

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

## Overview

The application consists of:

- **Aspire AppHost** - Orchestrates the Node.js API, Vite frontend, and Redis
- **YARP Reverse Proxy** - Routes requests to frontend and API
- **Vite Frontend** - React + TypeScript UI with real-time stats
- **Express API** - Node.js web API for tracking page visits
- **Redis** - In-memory data store for visit counts

## Architecture

```
┌─────────┐     ┌──────┐     ┌─────────────┐     ┌───────┐
│ Browser │────▶│ YARP │────▶│ Express API │────▶│ Redis │
└─────────┘     └──────┘     └─────────────┘     └───────┘
                    │
                    └────▶ Vite Dev Server (dev mode only)
```

In development, YARP proxies `/api/*` requests to Express and all other requests to Vite's dev server with HMR. In publish mode, YARP serves static frontend files and proxies API requests.

## Key Code

The AppHost configuration demonstrates YARP integration with JavaScript and Redis:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

var redis = builder.AddRedis("redis");

var api = builder.AddNodeApp("api", "./api", scriptPath: "index.js")
                 .WithHttpEndpoint(env: "PORT")
                 .WithHttpHealthCheck("/health")
                 .WaitFor(redis)
                 .WithReference(redis);

var frontend = builder.AddViteApp("frontend", "./frontend")
                      .WithReference(api);

// YARP handles routing and static file serving
builder.AddYarp("app")
       .WithConfiguration(c =>
       {
           // Always proxy /api requests to Express backend
           c.AddRoute("api/{**catch-all}", api)
            .WithTransformPathRemovePrefix("/api");

           if (builder.ExecutionContext.IsRunMode)
           {
               // In dev mode, proxy all other requests to Vite dev server
               c.AddRoute("{**catch-all}", frontend);
           }
       })
       .WithExternalHttpEndpoints()
       .PublishWithStaticFiles(frontend)
       .WithExplicitStart();

builder.Build().Run();
```

Key features:

- **YARP Integration**: Single endpoint for frontend and API with path transforms
- **Dual-Mode Operation**: Dev mode uses Vite HMR, publish mode serves static files
- **JavaScript Polyglot Support**: Uses `AddNodeApp` and `AddViteApp` for Node.js applications
- **Redis Integration**: Aspire provides connection properties via `REDIS_URI` environment variable
- **Package Manager Support**: Automatically runs `npm install` during development
- **Health Checks**: API health check with Redis connectivity test
- **Startup Dependencies**: API waits for Redis before starting
- **Docker Compose Deployment**: Ready for containerized deployment

## API Endpoints

- `GET /` - API information
- `GET /health` - Health check with Redis connectivity test
- `POST /visit/:page` - Record a visit to a page
- `GET /visit/:page` - Get visit count for a page
- `GET /stats` - Get stats for all pages
- `DELETE /stats` - Reset all statistics

## Example Usage

**Using the Web UI:**

Open the application URL shown in the Aspire dashboard (the YARP `app` endpoint). The UI displays interactive page cards with visit counts:
- **Click on any page card to record a visit** - the count increments in real-time
- Six predefined pages are always visible: home, about, contact, products, services, blog
- Stats update automatically every 5 seconds
- Reset all statistics with the "Reset All" button

**Using the API directly:**

```bash
# Record visits
curl -X POST http://localhost:[port]/api/visit/home
curl -X POST http://localhost:[port]/api/visit/home
curl -X POST http://localhost:[port]/api/visit/about

# Get visit count for a page
curl http://localhost:[port]/api/visit/home
# Returns: {"page":"home","visits":2}

# Get all stats
curl http://localhost:[port]/api/stats
# Returns: {"totalPages":2,"stats":{"home":2,"about":1}}
```

*Note: Replace `[port]` with the actual port from the Aspire dashboard.*

## How It Works

1. **Dependency Installation**: Aspire automatically runs `npm install` for both frontend and API
2. **Redis Connection**: Aspire provides the Redis connection via `REDIS_URI` environment variable (non-.NET connection property pattern)
3. **YARP Routing**:
   - Requests to `/api/*` are routed to Express with `/api` prefix removed
   - In dev mode, all other requests go to Vite dev server with HMR
   - In publish mode, static files are served from the frontend build
4. **Page Links**: Frontend displays a fixed set of page links (home, about, contact, products, services, blog)
5. **Visit Tracking**: Clicking a page increments its counter in Redis using the `INCR` command
6. **State Persistence**: Redis keeps all visit counts in memory for fast access
7. **Stats Aggregation**: The `/stats` endpoint queries all keys to build a complete view
8. **Real-Time UI**: Frontend polls `/api/stats` every 5 seconds to display updated visit counts

## Deployment

Deploy to Docker Compose:

```bash
aspire deploy
```

This will:

1. Build the Vite frontend to static files
2. Generate a Dockerfile for the Node.js API
3. Install npm dependencies and build the container image
4. Generate Docker Compose files with Redis
5. Deploy the complete application stack with YARP serving static files and proxying API requests
