# OpenMcp - .NET Dockerized MCP Servers

A collection of production-ready, Docker-native Model Context Protocol (MCP) servers built in C#.
Designed for Agentic RAG and Microservices architectures.

## 🚀 Quick Start

1. **Clone the repo**
2. **Run the stack**
   ```bash
   docker compose up --build
   ```

## 📦 Included Servers

### 1. PostgreSQL MCP Server (`:8081`)
*   **Type:** Database Tool
*   **Transport:** SSE (Server-Sent Events)
*   **Features:** Read-only safety mode, Schema inspection.
*   **Env:** `DATABASE_URL`

### 2. FileSystem MCP Server (`:8082`)
*   **Type:** File Operation Tool
*   **Transport:** SSE
*   **Features:** Sandboxed execution (jail) to a specific docker volume.
*   **Env:** Mount your data to `/data` inside the container.

### 3. DuckDuckGo Search MCP Server (`:8083`)
*   **Type:** Web Search Tool
*   **Transport:** SSE
*   **Features:** No API Key required, Privacy-focused.

## 🛠 Development

The `docker-compose.yml` includes a `test-db` (Postgres 16) for testing the Postgres MCP server locally without external dependencies.

### Testing with cURL
You can verify the servers are running by hitting their SSE endpoints:

```bash
curl -N http://localhost:8083/sse
```

## 🐳 Docker Hub

Images are available at `stockbix/mcp-server-*` (Coming Soon)