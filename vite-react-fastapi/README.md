# Vite React + FastAPI Sample

**React frontend with Vite + Python FastAPI backend demonstrating polyglot fullstack development.**

This sample showcases Aspire 13's polyglot support with a modern fullstack application combining JavaScript/React and Python/FastAPI.

## Quick Start

### Prerequisites

- [Aspire CLI](https://aspire.dev/get-started/install-cli/)
- [Docker](https://docs.docker.com/get-docker/)

### Commands

**Run locally** (automatically installs dependencies for both frontend and backend):

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

- **Aspire AppHost** - Orchestrates the React frontend and Python API
- **Vite + React Frontend** - Modern React application with HMR
- **FastAPI Backend** - Python REST API for todo management

## Key Code

The AppHost configuration demonstrates polyglot fullstack development with YARP:

```csharp
using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("dc");

var api = builder.AddUvicornApp("api", "./api", "main:app")
                 .WithHttpHealthCheck("/health");

var frontend = builder.AddViteApp("frontend", "./frontend")
                      .WithReference(api);

// Use YARP to serve frontend in dev mode and static files in publish mode
builder.AddYarp("app")
       .WithConfiguration(c =>
       {
           // Always proxy /api requests to FastAPI backend
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

- **Polyglot Fullstack**: Combines JavaScript (Vite + React) and Python (FastAPI)
- **YARP Reverse Proxy**: Routes requests to frontend or API based on path
- **Path Transform**: `.WithTransformPathRemovePrefix("/api")` strips `/api` prefix before forwarding to FastAPI
- **Health Checks**: HTTP health check configured for the Python API at `/health`
- **Dual-Mode Operation**:
  - **Dev Mode**: Routes UI to Vite dev server with HMR, `/api/*` to FastAPI
  - **Publish Mode**: Serves static files from built frontend, `/api/*` to FastAPI
- **Hot Module Replacement**: Vite dev server with instant updates in dev mode
- **Container Files as Build Artifacts**: Frontend build extracted from container
- **Single Entry Point**: YARP provides unified access to both frontend and API
- **Explicit Start**: YARP waits for dependencies to be ready before starting

## Application Features

The todo application demonstrates:

- **CRUD Operations**: Create, read, update, and delete todos
- **State Management**: React hooks for local state
- **API Integration**: Fetch API for backend communication
- **Responsive UI**: Clean, modern interface with CSS styling

## How It Works

1. **YARP Routing & Path Transform**:
   - **Dev Mode**:
     - YARP routes `/api/*` to FastAPI (strips `/api` prefix via `WithTransformPathRemovePrefix`)
     - All other requests go to Vite dev server
     - Vite also has a proxy configured for `/api/*` → FastAPI (for direct access)
   - **Publish Mode**:
     - YARP routes `/api/*` to FastAPI (strips `/api` prefix)
     - All other requests serve static files built from the frontend

2. **Path Transform Example**:
   - Client requests: `GET /api/todos`
   - YARP receives: `/api/todos`
   - Transform strips prefix: `/todos`
   - FastAPI receives: `GET /todos`

3. **Service Discovery**: Aspire automatically configures YARP and Vite proxy with endpoint information

4. **Development Experience**:
   - Aspire creates `.venv` for Python API and installs dependencies automatically
   - Aspire runs `npm install` for frontend and manages node_modules
   - Vite dev server provides HMR for instant frontend updates
   - Health checks ensure API is ready before YARP starts

5. **Frontend-Backend Communication**:
   - React components call `/api/*` endpoints via fetch
   - YARP or Vite proxy forwards to FastAPI with prefix removed

6. **Production Build**:
   - Vite builds static files extracted from the container
   - YARP serves static files and proxies API calls

## VS Code Integration

This sample includes VS Code configuration for Python development:

- **`.vscode/settings.json`**: Configures the Python interpreter to use the Aspire-created virtual environment
- After running `aspire run`, open the sample in VS Code for full IntelliSense and debugging support for the Python API
- The virtual environment at `api/.venv` will be automatically detected

## Deployment

Deploy the complete fullstack app to Docker Compose:

```bash
aspire deploy
```

This will:

1. Build the Vite frontend (static files)
2. Generate Dockerfiles for both apps
3. Install Python and npm dependencies
4. Create container images
5. Deploy the complete stack

## Development Tips

- Access the app via YARP endpoint (not individual frontend/backend endpoints)
- Frontend runs on Vite dev server with HMR in dev mode
- Backend auto-reloads on Python file changes
- YARP handles routing - no CORS configuration needed
- All components appear in the Aspire Dashboard with logs and metrics
- In publish mode, YARP serves the built frontend and proxies API calls
