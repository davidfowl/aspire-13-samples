# Aspire 13 Samples

Small, focused samples demonstrating .NET Aspire 13's polyglot platform support for Python, JavaScript, C#, and Go.

**Quick Start:** `cd <sample> && aspire run`

**Prerequisites:** [Aspire CLI](https://aspire.dev/get-started/install-cli/), [Docker](https://docs.docker.com/get-docker/)

## Samples

### AI & Machine Learning

**[rag-document-qa-svelte](./rag-document-qa-svelte)** - RAG document Q&A with Svelte
Upload documents and ask questions using retrieval augmented generation. Vector search with Qdrant, OpenAI embeddings. **(OpenAI, Qdrant, Svelte, Python, vector database, RAG pattern)**

### Python

**[python-fastapi-postgres](./python-fastapi-postgres)** - FastAPI + PostgreSQL + pgAdmin
CRUD API with async operations and database migrations. **(FastAPI, PostgreSQL, pgAdmin, async/await, `.WaitFor()` dependencies, requirements.txt)**

**[python-openai-agent](./python-openai-agent)** - OpenAI chat agent with web UI
AI-powered chat agent with streaming responses. **(OpenAI integration, uv package manager, pyproject.toml, API key management)**

**[python-script](./python-script)** - Pure Python script
Minimal Python script with zero dependencies. **(Virtual environment auto-creation, simplest possible Aspire app)**

### JavaScript

**[node-express-redis](./node-express-redis)** - Express + Redis + Vite frontend
Visit counter with real-time updates and clickable page cards. **(Express, Redis, Vite+React, YARP routing, WebSockets, instant state updates)**

**[vite-yarp-static](./vite-yarp-static)** - YARP serving static files
Single-file AppHost demonstrating static file serving. **(YARP reverse proxy, Vite HMR in dev, static file publishing, container files)**

### C#

**[vite-csharp-postgres](./vite-csharp-postgres)** - ASP.NET Core Minimal API + PostgreSQL + Vite
Todo app demonstrating modern .NET patterns. **(EF Core 10, PostgreSQL, pgAdmin, Scalar API docs, Vite+React, OpenTelemetry, container file publishing)**

**[vite-csharp-keycloak](./vite-csharp-keycloak)** - BFF with Keycloak authentication
BFF (Backend for Frontend) pattern with cookie-based OIDC authentication. Demonstrates auto-generated client secrets, realm configuration with environment variable substitution, and dual-mode redirect URLs (Vite dev server in run mode, BFF in publish mode). **(Keycloak, OIDC, PKCE, auto-generated secrets, realm import with variables, Aspire.Keycloak.Authentication, Vite+React, parameter generation)**

### Go

**[golang-api](./golang-api)** - Go API with in-memory storage
REST API with chi router and thread-safe in-memory data store. Custom Go integration that downloads modules, runs apps, and builds containers. **(Go, chi router, CRUD operations, sync.RWMutex, AddGoApp integration)**

### Polyglot

**[polyglot-task-queue](./polyglot-task-queue)** - Multi-language task queue with RabbitMQ + OpenTelemetry
Distributed task processing with Python, C#, and Node.js workers. **(RabbitMQ, distributed tracing, OpenTelemetry, messaging semantic conventions, polyglot services, Vite+React, language-specific strengths: pandas/numpy, strong typing, async I/O)**

**[vite-react-fastapi](./vite-react-fastapi)** - React + FastAPI fullstack
Todo app with YARP routing between Python backend and JavaScript frontend. **(FastAPI, Vite+React, YARP path transforms, dual-mode dev/publish)**

### Azure

**[image-gallery](./azure/image-gallery)** - Image gallery with event-triggered Container Apps Jobs
Upload images with queue-triggered thumbnail generation. Event-driven Container Apps Jobs with queue-based autoscaling, managed identity authentication, and Azure SQL free tier. Demonstrates Azure.Provisioning APIs for fine-grained infrastructure control. Runs locally with Azurite emulator - **no Azure subscription required for local development**. Can deploy entirely within Azure free tier limits. **(Azure Blob Storage, Azure SQL free tier, Azure Storage Queues, Container Apps Jobs, Vite+React, ImageSharp, Azure.Provisioning, managed identity)**

## Learn More

- [Aspire 13 Documentation](https://aspire.dev/whats-new/aspire-13/)
- [Aspire VS Code Extension](https://marketplace.visualstudio.com/items?itemName=microsoft-aspire.aspire-vscode)
- [Aspire GitHub](https://github.com/dotnet/aspire)
