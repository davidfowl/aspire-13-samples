# Aspire 13 Samples

A collection of small, focused samples demonstrating the key features of .NET Aspire 13.

## Prerequisites

- [.NET Aspire CLI](https://aspire.dev/get-started/install-cli/)
- [Docker](https://docs.docker.com/get-docker/)
- [Python](https://www.python.org/) (for Python samples)
- [Node.js](https://nodejs.org/) (for JavaScript samples)

## Quick Start

Each sample can be run independently:

```bash
cd <sample-directory>
aspire run
```

## Samples

### Python Samples

#### [python-fastapi-postgres](./python-fastapi-postgres)
FastAPI REST API with PostgreSQL database and pgAdmin.

**Features:**
- Python polyglot support with `AddUvicornApp`
- PostgreSQL database integration with pgAdmin web UI
- CRUD API with async operations and modular architecture
- Startup dependencies with `.WaitFor()` to ensure database is ready
- Uses `requirements.txt` for dependency management

**Run:**
```bash
cd python-fastapi-postgres
aspire run
# Access pgAdmin from the Aspire Dashboard to manage the database
```

#### [python-openai-agent](./python-openai-agent)
Python FastAPI AI agent with OpenAI integration and web UI.

**Features:**
- **Web Chat UI**: Clean, modern interface for chatting with AI
- Python AI workloads with OpenAI SDK
- **uv Package Manager**: Fast dependency installation from `pyproject.toml`
- Aspire's `AddOpenAI` for secure API key management (prompts for key on first run)
- Session-based conversation history
- REST API for programmatic access

**Run:**
```bash
cd python-openai-agent
aspire run
# Aspire will prompt for your OpenAI API key
# Open the AI agent endpoint in your browser to access the chat UI
```

#### [python-script](./python-script)
Simple Python script with zero dependencies demonstrating Aspire's virtual environment management.

**Features:**
- **Automatic Virtual Environment**: Aspire creates and manages `.venv`
- **No Dependencies**: Pure Python script with no external packages
- **Zero Configuration**: Just point to your Python script and run
- Console output in Aspire Dashboard
- No containers - runs directly with Python

**Run:**
```bash
cd python-script
aspire run
```

### JavaScript Samples

#### [node-express-redis](./node-express-redis)
Express API with Redis caching and React TypeScript frontend using YARP.

**Features:**
- **Vite + React + TypeScript**: Interactive frontend - click page cards to record visits
- **YARP Integration**: Single endpoint for frontend and API with path transforms
- **Dual-Mode Operation**: Dev mode uses Vite HMR, publish mode serves static files
- **Real-Time Updates**: Stats automatically refresh every 5 seconds
- JavaScript polyglot support with `AddNodeApp` and `AddViteApp`
- Redis integration for visit counter state management
- Health checks and startup dependencies
- npm package manager support

**Run:**
```bash
cd node-express-redis
aspire run
# Open the 'app' endpoint and click any page card to record visits
```

#### [yarpstatic](./yarpstatic)
YARP reverse proxy serving Vite static files.

**Features:**
- Single-file AppHost pattern
- Vite.js integration with HMR
- Container files as build artifacts
- Dual mode (dev/production) routing

**Run:**
```bash
cd yarpstatic
aspire run
```

### Polyglot Fullstack

#### [vite-react-fastapi](./vite-react-fastapi)
React frontend with Vite + Python FastAPI backend using YARP.

**Features:**
- Fullstack polyglot development (JavaScript + Python)
- YARP reverse proxy for routing
- Vite + React frontend with HMR in dev mode
- FastAPI backend
- Dual-mode operation (dev/publish)
- Container files as build artifacts
- Todo application with CRUD operations

**Run:**
```bash
cd vite-react-fastapi
aspire run
```

## Key Aspire 13 Features Demonstrated

### Polyglot Platform Support

#### Python Integration
Aspire's Python integration provides **zero-configuration virtual environment management**:

- **Automatic `.venv` Creation**: Aspire creates and manages virtual environments for each Python app
- **Dependency Installation**: Automatically installs from `requirements.txt` (pip) or `pyproject.toml` (uv)
- **uv Support**: Use `.WithUv()` for ultra-fast dependency installation (10-100x faster than pip)
- **Isolated Environments**: Each Python app gets its own virtual environment
- **No Manual Setup**: Just point to your Python code - Aspire handles virtual env, dependencies, and execution
- **Support for**: FastAPI (`AddUvicornApp`), scripts (`AddPythonApp`), modules, and executables

#### JavaScript Integration
- **Unified API**: `AddJavaScriptApp` works with any JavaScript runtime
- **Vite Specialization**: `AddViteApp` for React/Vue/Svelte with HMR support
- **Package Managers**: Automatic detection and use of npm, yarn, pnpm
- **Hot Module Replacement**: Vite dev server with instant updates

#### Connection Properties for Non-.NET Apps
- **PostgreSQL**: `DB_URI`, `DB_HOST`, `DB_PORT`, `DB_USERNAME`, `DB_PASSWORD`
- **Redis**: `REDIS_URI`, `REDIS_HOST`, `REDIS_PORT`
- **Standardized**: All connection info available as individual environment variables

### Container & Build Features
- Container files as build artifacts (extract frontend builds)
- Dynamic Dockerfile generation for Python and JavaScript
- Single-file AppHost with `#:package` directives
- YARP for dual-mode static file serving (dev/publish)

### Integration Features
- PostgreSQL, Redis integration with connection properties
- OpenAI SDK integration with secure credential management
- Service discovery across languages (Python ↔ C# ↔ JavaScript)
- Health checks and explicit start ordering

### Developer Experience
- **Python**: Automatic virtual environment creation and management
- **JavaScript**: Hot Module Replacement (HMR) for Vite
- **Automatic Dependency Installation**: No manual `pip install` or `npm install` needed
- **VS Code Integration**: All Python samples include `.vscode/settings.json` for automatic virtual environment detection
- **Aspire Dashboard**: Unified view of logs, metrics, and traces across all languages
- **External HTTP Endpoints**: Easy testing of all services
- **Service References**: Automatic endpoint injection via environment variables

## Sample Organization

All samples follow a consistent structure:

```
sample-name/
├── apphost.cs           # Single-file AppHost with package directives
├── apphost.run.json     # Launch settings
├── README.md            # Sample documentation
├── .vscode/             # VS Code settings (Python samples)
│   └── settings.json    # Python virtual environment configuration
└── <components>/        # Application components (api/, frontend/, etc.)
```

### VS Code Support

All Python samples include `.vscode/settings.json` that:
- Points to the Aspire-created virtual environment (`.venv`)
- Enables IntelliSense and code completion
- Supports debugging Python applications
- Works regardless of which folder you open in VS Code (uses relative paths)

## Common Commands

**Run locally:**
```bash
aspire run
```

**Deploy to Docker Compose:**
```bash
aspire deploy
```

**Teardown deployment:**
```bash
aspire do docker-compose-down-dc
```

**Build container images:**
```bash
aspire do build
```

## Learn More

- [Aspire Documentation](https://aspire.dev/)
- [What's New in Aspire 13](https://aspire.dev/whats-new/aspire-13/)
- [Aspire GitHub](https://github.com/dotnet/aspire)

## Contributing

These samples are designed to be small, focused, and language-specific. Each sample demonstrates specific Aspire 13 features without unnecessary complexity.
