# Aspire Static Files Sample

This sample demonstrates how to use **YARP (Yet Another Reverse Proxy)** with **Aspire** to serve static files from a Vite.js frontend application.

## Overview

This project showcases the integration of:

- **Aspire AppHost** - For orchestrating the distributed application
- **YARP (Yet Another Reverse Proxy)** - For reverse proxy functionality and static file serving
- **Vite.js** - For the frontend development and build tooling
- **Static File Publishing** - Demonstrating how to serve built frontend assets through YARP

## Architecture

The application consists of:

- **Aspire AppHost** - The Aspire application host that:
  - Configures a Vite.js frontend project with automatic dependency management
  - Sets up YARP as a reverse proxy
  - Publishes the frontend's static files through YARP
  - Configures external HTTP endpoints

- **Frontend (`frontend/`)** - A simple Vite.js application with:
  - Basic HTML structure
  - JavaScript with a counter component
  - CSS styling
  - Standard Vite development setup

## Key Features

- **Static File Serving**: YARP serves the built frontend assets
- **Development Integration**: Vite.js integration with Aspire for development workflow  
- **External Endpoints**: YARP configured with external HTTP endpoints for public access
- **Container Support**: Includes container orchestration environment configuration
- **Automatic Dependency Management**: Aspire handles frontend package installation automatically

## Project Structure

```text
yarpstatic/
├── apphost.cs              # Aspire AppHost configuration
├── apphost.run.json        # Launch settings
├── frontend/               # Vite.js frontend application
│   ├── index.html         # Main HTML file
│   ├── package.json       # Node.js dependencies and scripts
│   ├── src/
│   │   ├── main.js        # Main JavaScript entry point
│   │   ├── counter.js     # Counter component
│   │   └── style.css      # Styling
│   └── public/            # Public assets
└── README.md              # This file
```

## Getting Started

### Prerequisites

- [Aspire CLI](https://aspire.dev/get-started/install-cli/) (install with `curl -sSL https://aspire.dev/install.sh | bash`)
- Supported container runtime (docker, podman or rancher) (for container orchestration environment)

> **Note**: Aspire 13+ automatically handles .NET SDK and Node.js dependencies, including package installation.

### Running the Application

1. **Clone the repository** (if applicable):

   ```bash
   git clone <repository-url>
   cd yarpstatic
   ```

2. **Run the Aspire application**:

   ```bash
   aspire run
   ```

   Aspire will automatically:
   - Install Node.js dependencies (`npm install`)
   - Build the Vite frontend when needed
   - Start all services with proper orchestration

3. **Access the application**:
   - The Aspire dashboard will be available at the URL shown in the terminal
   - The YARP proxy with static files will be accessible through the configured endpoints

## How It Works

1. **Aspire Orchestration**: The AppHost registers a Vite.js application and YARP proxy
2. **Automatic Package Management**: Aspire detects `package.json` and runs `npm install` automatically
3. **Static File Publishing**: YARP is configured to serve static files from the Vite build output
4. **External Endpoints**: YARP exposes external HTTP endpoints for public access
5. **Development Workflow**: During development, Vite serves files; in production, YARP serves the built assets

## Key Code Snippets

### AppHost Configuration

```csharp
var frontend = builder.AddViteApp("frontend", "./frontend");

builder.AddYarp("app")
       .WithExternalHttpEndpoints()
       .PublishWithStaticFiles(frontend);
```

This configuration:

- Registers the Vite frontend application
- Creates a YARP instance named "app"
- Configures external HTTP endpoints
- Sets up static file publishing from the frontend build output

## Use Cases

This sample is useful for scenarios where you need to:

- Serve a modern frontend (React, Vue, Angular, etc.) through YARP
- Implement a reverse proxy pattern with static file serving
- Integrate frontend build tools with .NET Aspire orchestration
- Create a production-ready setup for serving SPAs (Single Page Applications)

## Related Concepts

- **YARP**: Microsoft's reverse proxy toolkit for .NET
- **Aspire**: Cloud-ready app orchestration framework (formerly .NET Aspire)
- **Vite.js**: Fast frontend build tool and dev server
- **Static File Serving**: Efficient delivery of frontend assets
- **Container Files**: Aspire 13+ feature for extracting build artifacts between containers

## What's New in Aspire 13

This sample takes advantage of several Aspire 13 features:

- **Simplified AppHost**: Uses the new single-file AppHost format with `#:package` directives
- **JavaScript First-Class Support**: Vite.js applications are now first-class citizens
- **Automatic Package Management**: No manual `npm install` required - Aspire handles it
- **Container Files as Build Artifacts**: Support for extracting files from one container to another
- **Improved CLI**: Uses the new `aspire` CLI instead of `dotnet` commands

## Next Steps

To extend this sample, consider:

- Adding API endpoints behind YARP
- Implementing authentication and authorization
- Adding multiple frontend applications
- Configuring load balancing and health checks
- Adding monitoring and observability features
- Exploring Aspire 13's container files feature for more complex build scenarios