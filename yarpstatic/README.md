# Aspire Static Files Sample

This sample demonstrates how to use YARP (Yet Another Reverse Proxy) with Aspire to serve static files from a Vite.js frontend application.

## Overview

The application consists of:

- **Aspire AppHost** - Orchestrates the application and configures YARP
- **YARP** - Serves static files from the Vite.js frontend
- **Vite.js Frontend** - A simple JavaScript application with a counter component

## Getting Started

### Prerequisites

- [Aspire CLI](https://aspire.dev/get-started/install-cli/)
- Docker Compose

### Running the Application

1. Clone and navigate to the project:

   ```bash
   git clone <repository-url>
   cd yarpstatic
   ```

2. Run the application:

   ```bash
   aspire run
   ```

3. Access the application through the URLs shown in the Aspire dashboard.

## Key Code

The AppHost configuration is straightforward:

```csharp
var frontend = builder.AddViteApp("frontend", "./frontend");

builder.AddYarp("app")
       .WithExternalHttpEndpoints()
       .PublishWithStaticFiles(frontend);
```

This registers the Vite frontend and configures YARP to serve its static files with external HTTP endpoints.
