# RAG Document Q&A with Svelte

A Retrieval Augmented Generation (RAG) system demonstrating **vector search** with Qdrant and **Svelte** for a lightweight, modern frontend.

## What This Demonstrates

- **RAG Pattern**: Retrieval Augmented Generation for accurate, source-backed answers
- **Vector Database**: Qdrant for semantic search with embeddings
- **OpenAI Integration**: Embeddings (text-embedding-3-small) and chat completion (GPT-4o-mini)
- **Document Processing**: Text chunking with token-based splitting
- **Svelte Frontend**: Lightweight, compiler-based framework (alternative to React)
- **Polyglot Stack**: Python (FastAPI) backend + JavaScript (Svelte) frontend

## Architecture

```
┌──────────────┐
│    Svelte    │
│   Frontend   │
└──────┬───────┘
       │ HTTP
       ▼
┌──────────────┐      ┌───────────┐      ┌──────────┐
│   FastAPI    │─────→│  Qdrant   │      │  OpenAI  │
│   (Python)   │      │  Vector   │      │   API    │
│              │      │    DB     │      └──────────┘
└──────────────┘      └───────────┘
       │
       └─→ 1. Get embeddings
       └─→ 2. Store vectors
       └─→ 3. Semantic search
       └─→ 4. Generate answer
```

## How RAG Works

### 1. Document Upload & Indexing

```python
Document → Chunk (500 tokens) → Embed (OpenAI) → Store (Qdrant)
```

- Upload `.txt` files via drag-and-drop or file picker
- Documents are split into 500-token chunks with 50-token overlap
- Each chunk is converted to a 1536-dimension embedding vector
- Vectors are stored in Qdrant with metadata (filename, chunk index, text)

### 2. Question Answering

```python
Question → Embed → Search (top 3 chunks) → Context → GPT → Answer
```

- User question is converted to an embedding vector
- Qdrant performs cosine similarity search to find top 3 relevant chunks
- Retrieved chunks are used as context for GPT-4o-mini
- AI generates an answer based only on the context
- Sources are shown with similarity scores

## Key Features

### Vector Search
- **Semantic similarity**, not keyword matching
- Finds relevant content even with different wording
- Example: "How do I configure Redis?" matches chunks about "setting up cache"

### Source Attribution
- Every answer shows which document chunks were used
- Similarity scores show relevance (0-100%)
- Prevents hallucination by grounding answers in actual documents

### Token-Aware Chunking
Uses `tiktoken` for accurate token counting:
- Chunks are 500 tokens (not characters)
- 50-token overlap preserves context at boundaries
- Ensures efficient use of embedding API

## Running the Sample

1. **Prerequisites**:
   - .NET 10 SDK
   - Docker
   - Python 3.12+
   - Node.js 20+
   - OpenAI API key
   - `uv` package manager: `pip install uv`

2. **Set OpenAI API key**:
   ```bash
   export OPENAI_APIKEY="your-key-here"
   ```

3. **Run**:
   ```bash
   aspire run
   ```

4. **Access**: Open the frontend endpoint from Aspire dashboard

## Usage

1. **Upload Documents**:
   - Drag and drop `.txt` files or click to browse
   - Watch as documents are chunked and indexed
   - See indexed documents in the list

2. **Ask Questions**:
   - Type a question about your documents
   - AI retrieves relevant chunks from Qdrant
   - Get an answer with source attribution
   - See similarity scores for transparency

## Example Documents to Try

**example.txt**:
```
.NET Aspire is a cloud-ready stack for building distributed applications.
It provides automatic service discovery, configuration management, and telemetry.
Aspire supports Python, JavaScript, and C# applications with polyglot hosting.
```

**Questions**:
- "What languages does Aspire support?"
- "What does Aspire provide?"
- "Is Aspire good for microservices?"

## Svelte vs React

| Feature | Svelte | React |
|---------|--------|-------|
| **Runtime** | No virtual DOM | Virtual DOM diffing |
| **Bundle Size** | ~4KB | ~42KB |
| **Syntax** | Cleaner, less boilerplate | JSX with hooks |
| **Reactivity** | Compiler-based | Runtime with useState |
| **Learning Curve** | Gentler | Steeper |

**Svelte Example**:
```svelte
<script>
  let count = $state(0);
</script>

<button onclick={() => count++}>
  Clicked {count} times
</button>
```

**React Equivalent**:
```jsx
const [count, setCount] = useState(0);

return (
  <button onClick={() => setCount(count + 1)}>
    Clicked {count} times
  </button>
);
```

## Project Structure

```
aspire-rag-document-qa-svelte/
├── apphost.cs               # Aspire orchestration
├── api/                     # Python FastAPI
│   ├── pyproject.toml
│   └── main.py             # RAG backend with Qdrant + OpenAI
└── frontend/                # Svelte app
    ├── package.json
    ├── vite.config.js
    ├── svelte.config.js
    └── src/
        ├── App.svelte      # Main component
        ├── main.js
        └── app.css
```

## Technologies

- **Python**: FastAPI for REST API
- **OpenAI**: text-embedding-3-small (embeddings), gpt-4o-mini (chat)
- **Qdrant**: Vector database for semantic search
- **Svelte 5**: Modern, lightweight frontend framework
- **Vite**: Fast build tool and dev server
- **.NET Aspire**: Service orchestration and configuration

## Learn More

- [RAG Pattern Explained](https://www.pinecone.io/learn/retrieval-augmented-generation/)
- [Qdrant Documentation](https://qdrant.tech/documentation/)
- [Svelte Documentation](https://svelte.dev/)
- [OpenAI Embeddings Guide](https://platform.openai.com/docs/guides/embeddings)
- [Aspire Python Integration](https://learn.microsoft.com/dotnet/aspire/get-started/build-aspire-apps-with-python)
