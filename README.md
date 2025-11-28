# Aspire Samples

Polyglot samples for building, running, and wiring real apps across Python, JavaScript, C#, and Go.

Aspire orchestrates your services, manages connection strings, and brings everything up with a single command—regardless of language.

**Quick Start:** `cd <sample> && aspire run`

**Prerequisites:** [Aspire CLI](https://aspire.dev/get-started/install-cli/), [Docker](https://docs.docker.com/get-docker/)

## Samples

### Fullstack Web Apps

**[vite-react-fastapi](./vite-react-fastapi)** - React + FastAPI fullstack *(Python, JavaScript)*
Todo app with React frontend and FastAPI backend. Shows how Aspire coordinates frontend and backend with YARP routing and handles dev/publish modes seamlessly. **(FastAPI, Vite+React, YARP path transforms, dual-mode dev/publish)**

**[vite-csharp-postgres](./vite-csharp-postgres)** - Minimal API + PostgreSQL + Vite *(C#, JavaScript)*
Todo app with React frontend and C# backend. Shows how Aspire wires the API to PostgreSQL, injects connection strings, and serves the frontend through YARP. **(EF Core 10, PostgreSQL, pgAdmin, Scalar API docs, Vite+React, OpenTelemetry, container file publishing)**

**[node-express-redis](./node-express-redis)** - Express + Redis + Vite frontend *(JavaScript)*
Visit counter with real-time updates. Shows how Aspire connects Express to Redis and routes traffic through YARP with WebSocket support. **(Express, Redis, Vite+React, YARP routing, WebSockets, instant state updates)**

### APIs & Services

**[golang-api](./golang-api)** - Go API with in-memory storage *(Go)*
REST API with chi router. Shows how Aspire runs Go apps, downloads modules, and builds containers—all with a custom Go integration. **(Go, chi router, CRUD operations, sync.RWMutex, AddGoApp integration)**

**[python-fastapi-postgres](./python-fastapi-postgres)** - FastAPI + PostgreSQL + pgAdmin *(Python)*
CRUD API with async operations. Shows how Aspire wires FastAPI to PostgreSQL and pgAdmin, manages connection strings, and brings everything up with `aspire run`. **(FastAPI, PostgreSQL, pgAdmin, async/await, `.WaitFor()` dependencies, requirements.txt)**

### AI & Agents

**[python-openai-agent](./python-openai-agent)** - OpenAI chat agent with web UI *(Python)*
AI-powered chat agent with streaming responses. Shows how Aspire manages API keys as parameters and coordinates the Python environment with uv. **(OpenAI integration, uv package manager, pyproject.toml, API key management)**

**[rag-document-qa-svelte](./rag-document-qa-svelte)** - RAG document Q&A with Svelte *(Python, JavaScript)*
Upload documents and ask questions using retrieval augmented generation. Shows how Aspire orchestrates Qdrant vector database, OpenAI embeddings, and a Svelte frontend together. **(OpenAI, Qdrant, Svelte, Python, vector database, RAG pattern)**

### Polyglot Systems

**[polyglot-task-queue](./polyglot-task-queue)** - Multi-language task queue with RabbitMQ *(Python, C#, JavaScript)*
Distributed task processing with Python, C#, and Node.js workers. Shows how Aspire runs workers in multiple languages against the same RabbitMQ broker and correlates traces across all of them with OpenTelemetry. **(RabbitMQ, distributed tracing, OpenTelemetry, messaging semantic conventions, polyglot services, Vite+React)**

### Cloud & Azure

**[image-gallery](./azure/image-gallery)** - Image gallery with event-triggered Container Apps Jobs *(C#, JavaScript)*
Upload images with queue-triggered thumbnail generation. Shows how Aspire provisions Azure resources and runs locally with Azurite—no Azure subscription required for development. **(Azure Blob Storage, Azure Storage Queues, Container Apps Jobs, Vite+React, Azure.Provisioning)**

### Foundations

**[python-script](./python-script)** - Pure Python script *(Python)*
Minimal Python script with zero dependencies. Shows the simplest possible Aspire app—auto-created virtual environment, single script, one command to run. **(Virtual environment auto-creation, simplest possible Aspire app)**

**[vite-yarp-static](./vite-yarp-static)** - YARP serving static files *(JavaScript)*
Single-file AppHost demonstrating static file serving. Shows how Aspire handles Vite HMR in dev and publishes static files for production. **(YARP reverse proxy, Vite HMR in dev, static file publishing, container files)**

## Learn More

- [Aspire Documentation](https://aspire.dev/docs/)
- [Aspire VS Code Extension](https://marketplace.visualstudio.com/items?itemName=microsoft-aspire.aspire-vscode)
- [Aspire GitHub](https://github.com/dotnet/aspire)
