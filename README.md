# Aspire 13 Samples

Small, focused samples demonstrating .NET Aspire 13's polyglot platform support for Python and JavaScript.

**Quick Start:** `cd <sample> && aspire run`

**Prerequisites:** [Aspire CLI](https://aspire.dev/get-started/install-cli/), [Docker](https://docs.docker.com/get-docker/)

## Samples

### Python

**[python-fastapi-postgres](./python-fastapi-postgres)** - FastAPI + PostgreSQL + pgAdmin
CRUD API with async operations. Uses `requirements.txt` and demonstrates `.WaitFor()` startup dependencies.

**[python-openai-agent](./python-openai-agent)** - OpenAI chat agent with web UI
AI workloads with `AddOpenAI` and uv package manager (`pyproject.toml`). Aspire prompts for API key.

**[python-script](./python-script)** - Pure Python script
Zero dependencies. Demonstrates automatic virtual environment creation and management.

### JavaScript

**[node-express-redis](./node-express-redis)** - Express + Redis + Vite frontend
Visit counter with clickable page cards. YARP routes frontend and API. Real-time updates with instant in-place count increments.

**[yarpstatic](./yarpstatic)** - YARP serving static files
Single-file AppHost pattern. Vite with HMR in dev, static files in publish. Container files as build artifacts.

### C#

**[csharp-minimal-postgres](./csharp-minimal-postgres)** - ASP.NET Core Minimal API + PostgreSQL + Vite
Todo app with EF Core 10, pgAdmin, and Scalar API docs. React TypeScript frontend with container files publishing. Observability with OpenTelemetry.

### Polyglot

**[polyglot-task-queue](./polyglot-task-queue)** - Multi-language task queue with RabbitMQ
Distributed task processing with Python, C#, and Node.js workers. Demonstrates message queues, language-specific strengths (pandas, strong typing, async I/O), and Vite frontend with real-time updates.

**[vite-react-fastapi](./vite-react-fastapi)** - React + FastAPI fullstack
Todo app with YARP routing. Python backend, JavaScript frontend. Path transforms and dual-mode operation.

## Learn More

- [Aspire 13 Documentation](https://aspire.dev/whats-new/aspire-13/)
- [Aspire VS Code Extension](https://marketplace.visualstudio.com/items?itemName=microsoft-aspire.aspire-vscode)
- [Aspire GitHub](https://github.com/dotnet/aspire)
