# Polyglot Task Queue

A distributed task processing system demonstrating how to build polyglot microservices with .NET Aspire 13, RabbitMQ, and multiple programming languages (Python, C#, and Node.js).

## Overview

This sample showcases a real-world task queue architecture where different services written in different languages communicate via RabbitMQ to process tasks based on their strengths:

- **Node.js API**: Handles HTTP requests, manages task state, and coordinates message flow
- **Vite React Frontend**: Provides a user-friendly interface for task submission and monitoring
- **Python Worker**: Processes data analysis tasks using pandas and numpy
- **C# Worker**: Generates structured reports with strong typing and performance

## Architecture

```mermaid
graph TB
    subgraph Frontend
        UI[Vite React UI]
    end

    subgraph "Node.js API (2 Channels)"
        HTTP[HTTP Endpoints<br/>GET /tasks<br/>POST /tasks]
        Cache[(In-Memory Cache<br/>tasks Map)]
        PubChan[Publisher Channel]
        ConsChan[Consumer Channel<br/>Background Loop]
    end

    subgraph RabbitMQ
        TasksQ[tasks queue]
        StatusQ[task_status queue]
        ResultsQ[results queue]
    end

    subgraph Workers
        PythonW[Python Worker<br/>analyze tasks]
        CSharpW[C# Worker<br/>report tasks]
    end

    UI -->|HTTP GET| HTTP
    UI -->|HTTP POST| HTTP
    HTTP -->|Read Only| Cache
    HTTP -->|Publish| PubChan

    PubChan -->|queued status| StatusQ
    PubChan -->|task| TasksQ

    ConsChan -.->|Consume & Update Cache| StatusQ
    ConsChan -.->|Consume & Update Cache| ResultsQ
    ConsChan -.->|Update| Cache

    TasksQ -->|Consume| PythonW
    TasksQ -->|Consume| CSharpW

    PythonW -->|processing status| StatusQ
    PythonW -->|result| ResultsQ

    CSharpW -->|processing status| StatusQ
    CSharpW -->|result| ResultsQ

    style ConsChan fill:#e1f5ff,stroke:#0288d1
    style Cache fill:#fff9c4,stroke:#f57c00
    style HTTP fill:#c8e6c9,stroke:#388e3c
    style PubChan fill:#f8bbd0,stroke:#c2185b
```

**Key Points:**
- **Consumer Channel** (blue): Background loop that continuously consumes messages and updates cache
- **Publisher Channel** (pink): Publishes messages from HTTP requests
- **Cache** (yellow): In-memory Map, updated only by consumer channel, read by HTTP endpoints
- **HTTP Endpoints** (green): Read from cache only, never query RabbitMQ directly
- **Dotted lines**: Background consumer updates (continuous loop)
- **Solid lines**: Synchronous request/response or publish operations

### Message Flow

**RabbitMQ is the source of truth for all task state.** The Node.js API maintains an in-memory cache that is built entirely from consuming RabbitMQ messages.

1. **Task Submission**:
   - User submits a task through the frontend
   - Node.js API publishes task to `tasks` queue (for workers)
   - Node.js API publishes `queued` status to `task_status` queue (for state tracking)
   - API consumes status from `task_status` queue and adds to cache

2. **Task Processing**:
   - Workers consume tasks from `tasks` queue based on task type:
     - Python worker processes `analyze` tasks
     - C# worker processes `report` tasks
   - Workers publish `processing` status to `task_status` queue
   - API consumes status updates and updates cache

3. **Task Completion**:
   - Workers publish results to `results` queue
   - API consumes results, updates cache with final result
   - Frontend polls API and displays updated task status from cache

All state changes flow through RabbitMQ queues, making the system stateless and resilient to API restarts.

## What This Demonstrates

- **Polyglot Architecture**: Services in Python, C#, and Node.js working together
- **Message Queue Pattern**: Asynchronous task processing with RabbitMQ
- **Event-Driven State Management**:
  - RabbitMQ as the single source of truth for all task state
  - In-memory cache built from consuming messages
  - Stateless API that can restart without losing task state
  - All state changes published as events to queues
- **Distributed Tracing**:
  - End-to-end OpenTelemetry instrumentation across all services
  - Trace context propagation through RabbitMQ message headers
  - Messaging semantic conventions for all queue operations
  - Polyglot tracing (Node.js, Python, C#) in a single trace
- **Aspire Integration**:
  - `AddRabbitMQ()` with management plugin
  - `AddNodeApp()` for Node.js applications
  - `AddPythonApp()` for Python applications
  - `AddCSharpApp()` for C# applications
  - `AddViteApp()` for frontend applications
  - Automatic OpenTelemetry configuration and export
- **Service References**: Automatic connection string injection via environment variables
- **Language-Specific Strengths**:
  - Python: Data analysis with pandas/numpy
  - C#: Structured data processing with strong typing
  - Node.js: I/O coordination and HTTP handling
- **Real-time UI**: Frontend with polling for task status updates

## Project Structure

```
polyglot-task-queue/
├── apphost.cs                 # Aspire orchestration
├── apphost.run.json           # Launch settings
├── api/                       # Node.js Express API
│   ├── package.json
│   ├── index.js              # Task coordination and RabbitMQ integration
│   └── Properties/
│       └── launchSettings.json
├── frontend/                  # Vite React TypeScript frontend
│   ├── package.json
│   ├── vite.config.ts        # Vite config with API proxy
│   ├── tsconfig.json
│   ├── index.html
│   └── src/
│       ├── main.tsx          # React entry point
│       ├── App.tsx           # Main UI component
│       └── style.css         # Styling
├── worker-python/             # Python data analysis worker
│   ├── pyproject.toml
│   └── main.py               # aio-pika consumer with pandas processing
└── worker-csharp/             # C# report generation worker
    ├── worker-csharp.csproj
    └── Program.cs            # Aspire.RabbitMQ.Client consumer
```

## Running the Sample

1. **Prerequisites**:
   - .NET 10 SDK
   - Node.js 20+
   - Python 3.12+
   - Docker (for RabbitMQ)
   - `uv` (Python package manager): `pip install uv`

2. **Run with Aspire**:
   ```bash
   dotnet run
   ```

3. **Access the application**:
   - **Frontend**: Click the "Task Queue UI" link in Aspire dashboard
   - **RabbitMQ Management**: `http://localhost:15672` (guest/guest)
   - **Aspire Dashboard**: Displayed in console output

## State Management Architecture

This sample demonstrates an **event-driven state management** pattern where RabbitMQ is the single source of truth.

### Two-Channel Pattern

The Node.js API uses **two separate RabbitMQ channels** for different purposes:

1. **Consumer Channel** (Background Loop):
   - Runs continuously in the background
   - Consumes messages from `task_status` and `results` queues
   - Updates the in-memory cache (tasks Map)
   - **Never touched by HTTP requests**

2. **Publisher Channel** (HTTP Request Handler):
   - Used by HTTP endpoints to publish messages
   - Publishes to `task_status` queue (for state tracking)
   - Publishes to `tasks` queue (for worker distribution)
   - **Never used for reading data**

### Key Queues

1. **`tasks` queue**: Work queue for task distribution to workers
2. **`task_status` queue**: Event stream for all state changes (queued, processing, error, skipped)
3. **`results` queue**: Completed task results from workers

### Data Flow

```
┌─────────────┐
│ POST /tasks │ (HTTP Request)
└──────┬──────┘
       │
       ├──► Publisher Channel ──► task_status queue ──┐
       │                                               │
       └──► Publisher Channel ──► tasks queue ──► Worker
                                                      │
                                                      ▼
                                            processing status
                                                      │
                                                      ▼
                                            task_status queue ──┐
                                                                │
                                                                │
                    ┌───────────────────────────────────────────┤
                    │                                           │
                    ▼                                           ▼
            Consumer Channel                           Consumer Channel
          (Background Loop)                          (Background Loop)
                    │                                           │
                    ▼                                           ▼
            Update Cache ◄────────────────────────────── Update Cache
                    │
                    │
            ┌───────┴────────┐
            │                │
            ▼                ▼
       GET /tasks      GET /tasks/:id
    (Read from Cache) (Read from Cache)
```

### Separation of Concerns

**HTTP Endpoints:**
- `GET /tasks` - Reads from cache only
- `GET /tasks/:id` - Reads from cache only
- `POST /tasks` - Publishes to RabbitMQ via publisher channel, **does not touch cache**
- **Never query RabbitMQ for data**

**Background Consumer:**
- Runs continuously, independent of HTTP requests
- Consumes messages from RabbitMQ queues
- Updates cache as messages arrive
- **Only component that writes to cache**

### Cache Rebuilding

The Node.js API cache is **ephemeral and rebuilt from messages**:
- On startup, cache is empty
- As messages are consumed from `task_status` and `results` queues, cache is populated
- If API restarts, new state updates will rebuild the cache
- For production, you'd add a database for persistent storage alongside RabbitMQ

This architecture makes the system:
- **Stateless**: API can restart without data loss (messages remain in queues)
- **Resilient**: Workers can fail and retry without coordinator involvement
- **Auditable**: All state changes are events in queues
- **Scalable**: Multiple API instances can consume from the same queues
- **Performant**: HTTP requests only read from in-memory cache, never wait on RabbitMQ queries

## How It Works

### Task Submission

1. User selects task type (Data Analysis or Report Generation)
2. User enters data (CSV, JSON, or plain text)
3. Frontend POSTs to `/tasks` endpoint
4. API publishes `queued` status to `task_status` queue (source of truth)
5. API publishes task to `tasks` queue (for workers)
6. API consumes `queued` status and adds task to cache
7. Frontend receives immediate response and continues polling

### Task Processing

**Python Worker (Data Analysis)**:
- Consumes tasks with `type: "analyze"` from `tasks` queue
- Publishes `processing` status to `task_status` queue
- Parses input as CSV, JSON, or text
- Generates statistics:
  - Numeric columns: mean, median, std, min, max
  - Categorical columns: unique count, top values
  - Preview of first 5 rows
- Publishes results to `results` queue
- On error: publishes `error` status to `task_status` queue

**C# Worker (Report Generation)**:
- Consumes tasks with `type: "report"` from `tasks` queue
- Publishes `processing` status to `task_status` queue
- Processes text data
- Generates structured report with:
  - Summary statistics (lines, characters, averages)
  - Line-by-line analysis (first 10 lines)
  - Overall statistics (shortest/longest lines, word counts)
  - Metadata (timestamp, version, generator)
- Publishes results to `results` queue
- On error: publishes `error` status to `task_status` queue

### Result Handling

1. API consumes `processing` status → updates cache to show worker is processing
2. API consumes from `results` queue → updates cache with completed status and result
3. Frontend polls `/tasks` every 2 seconds → receives updated cache state
4. UI updates with task status, results, and worker badges

## Example Tasks

### Data Analysis (Python)

**Input (CSV)**:
```csv
name,age,city
Alice,30,NYC
Bob,25,SF
Charlie,35,LA
```

**Output**:
```json
{
  "row_count": 3,
  "column_count": 3,
  "columns": ["name", "age", "city"],
  "summary": {
    "numeric": {
      "age": {
        "mean": 30.0,
        "median": 30.0,
        "std": 5.0,
        "min": 25.0,
        "max": 35.0
      }
    },
    "categorical": {
      "name": { "unique_count": 3, "top_values": {...} },
      "city": { "unique_count": 3, "top_values": {...} }
    }
  },
  "preview": [...]
}
```

### Report Generation (C#)

**Input (Text)**:
```
This is a sample document
for report generation
using the C# worker
```

**Output**:
```json
{
  "title": "Report generated at 2025-01-15 10:30:00",
  "summary": {
    "totalLines": 3,
    "totalCharacters": 65,
    "averageLineLength": 21,
    "processedBy": "csharp-worker"
  },
  "content": { "lines": [...] },
  "statistics": {
    "shortestLine": 18,
    "longestLine": 27,
    "totalWords": 11
  },
  "metadata": {...}
}
```

## Key Aspire Features

### Connection String Injection

RabbitMQ connection details are automatically injected via environment variables:

- `MESSAGING_URI`: Full connection URI (e.g., `amqp://guest:guest@localhost:5672`)
- `MESSAGING_HOST`: Host name
- `MESSAGING_PORT`: Port number
- `MESSAGING_USERNAME`: Username
- `MESSAGING_PASSWORD`: Password

Each service accesses RabbitMQ using the appropriate client library:
- **Node.js**: `amqplib` with `process.env.MESSAGING_URI`
- **Python**: `aio-pika` with `os.environ['MESSAGING_URI']`
- **C#**: `Aspire.RabbitMQ.Client` with `builder.AddRabbitMQClient("messaging")`

### Service Orchestration

The AppHost coordinates all services:

```csharp
var rabbitmq = builder.AddRabbitMQ("messaging").WithManagementPlugin();
var api = builder.AddNodeApp("api", ...).WithReference(rabbitmq);
var frontend = builder.AddViteApp("frontend", ...).WithReference(api);
builder.AddPythonApp("worker-python", ...).WithReference(rabbitmq);
builder.AddCSharpApp("worker-csharp", ...).WithReference(rabbitmq);
api.PublishWithContainerFiles(frontend, "public");
```

### Health Checks

- **Node.js API**: `/health` endpoint
- **RabbitMQ**: Management plugin provides health monitoring
- **Workers**: Connection resilience with automatic reconnection

## Technologies Used

- **.NET Aspire 13**: Orchestration and service management
- **RabbitMQ**: Message broker for task queue
- **Node.js + Express**: API server and coordination
- **Vite + React + TypeScript**: Modern frontend
- **Python + aio-pika + pandas**: Data analysis worker
- **C# + Aspire.RabbitMQ.Client**: Report generation worker
- **OpenTelemetry**: End-to-end distributed tracing

## Distributed Tracing with OpenTelemetry

This sample demonstrates **end-to-end distributed tracing** across all polyglot services using OpenTelemetry.

### Trace Flow

A single task submission creates a distributed trace that spans:

```
Frontend (HTTP) → Node.js API → RabbitMQ → Python/C# Worker → RabbitMQ → Node.js API → Frontend
```

### What Gets Traced

#### **Node.js API** (`api/`)
- HTTP requests (auto-instrumented by Express)
- RabbitMQ publish operations with context injection
- RabbitMQ consume operations with context extraction
- Custom spans for task submission and processing
- **Messaging semantic conventions**: `messaging.system`, `messaging.destination.name`, `messaging.operation`, `task.id`

#### **Python Worker** (`worker-python/`)
- RabbitMQ message consumption with parent context extraction
- Data processing spans
- Result publishing with trace propagation
- **Messaging semantic conventions**: `messaging.system`, `messaging.source.name`, `messaging.operation`, `task.id`, `task.type`

#### **C# Worker** (`worker-csharp/`)
- RabbitMQ message consumption using `ActivitySource`
- Report generation child spans
- Result publishing with trace context
- **Messaging semantic conventions**: `messaging.system`, `messaging.source.name`, `messaging.operation`, `task.id`, `task.type`

### Trace Context Propagation

Trace context is propagated through **RabbitMQ message headers** using W3C Trace Context:

1. **API publishes task**: Injects trace context into message headers
2. **Worker consumes task**: Extracts trace context from headers, creates child span
3. **Worker publishes result**: Injects updated trace context into result headers
4. **API consumes result**: Extracts trace context, completes the trace

### Observing Traces

View distributed traces in the **Aspire Dashboard**:

1. Run the sample with `aspire run`
2. Open the Aspire Dashboard (URL shown in console)
3. Click on **Traces** tab
4. Submit a task through the frontend
5. See the complete trace showing:
   - HTTP request to POST /tasks
   - RabbitMQ publish to tasks queue
   - Worker processing span
   - RabbitMQ publish to results queue
   - API consuming result
   - HTTP response

### Messaging Semantic Conventions

All RabbitMQ operations follow [OpenTelemetry Messaging Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/messaging/):

- `messaging.system`: `rabbitmq`
- `messaging.operation`: `publish`, `process`, `receive`
- `messaging.destination.name`: Queue name (e.g., `tasks`, `results`, `task_status`)
- `messaging.source.name`: Source queue for consumers
- `messaging.message.id`: Task ID for correlation
- `task.id`: Custom attribute for task identification
- `task.type`: Custom attribute for task type (`analyze`, `report`)
- `task.status`: Custom attribute for task status (`queued`, `processing`, `completed`)

### Implementation Details

**Node.js**: Uses `@opentelemetry/sdk-node` with automatic instrumentation + manual spans for RabbitMQ

**Python**: Uses `opentelemetry-sdk` with manual span creation and context propagation

**C#**: Uses `ActivitySource` (built into .NET) with `Aspire.RabbitMQ.Client` automatic instrumentation

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/tutorials)
- [Aspire RabbitMQ Integration](https://learn.microsoft.com/dotnet/aspire/messaging/rabbitmq-component)
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [OpenTelemetry Messaging Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/messaging/)
