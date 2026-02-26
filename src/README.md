# OpenMcp - Python Dockerized MCP Servers for .Net, Semantic Kernel , C#

A collection of production-ready, Docker-native **Model Context Protocol (MCP)** servers.
These servers expose standard tools (Database, Files, Search) over SSE (Server-Sent Events) for AI Agents to consume.

## Quick Start

1.  **Clone the repo**
2.  **Configure Environment (Optional)**
    Copy `.env.example` to `.env` to override defaults.
3.  **Run the stack**
    ```bash
    docker compose up --build
    ```

##  Included Servers

### 1. PostgreSQL MCP Server (`:8081`)
*   **Image:** `openmcp/mcp-postgres`
*   **Description:** Allows agents to query PostgreSQL databases. Includes schema inspection tools.
*   **Configuration:**
    *   `DATABASE_URL`: Connection string (e.g., `postgres://user:pass@host:5432/db`).
    *   `POSTGRES_READ_ONLY`: `true` (default) blocks INSERT/UPDATE/DELETE/DROP commands.

### 2. FileSystem MCP Server (`:8082`)
*   **Image:** `openmcp/mcp-filesystem`
*   **Description:** Provides `read_file`, `write_file`, and `list_directory` tools.
*   **Configuration:**
    *   `FILESYSTEM_ROOT`: Internal path to serve (default `/data`).
*   **Volumes:**
    *   Mount your local folder to `/data` to give the agent access.
    *   Example: `- ./sandbox:/data`

### 3. DuckDuckGo Search MCP Server (`:8083`)
*   **Image:** `openmcp/mcp-duckduckgo`
*   **Description:** Performs web searches without an API key.
*   **Configuration:**
    *   `DDG_DEFAULT_MAX_RESULTS`: Default number of results (default `10`).

##  Development

The `docker-compose.yml` includes a `test-db` (Postgres 16) for testing locally without external dependencies.

### Verifying Connectivity
You can verify the servers are running by hitting their SSE endpoints. You should see a stream connection open.

```bash
curl -N http://localhost:8083/sse
```

## Docker Hub

Images are available at `openmcp/mcp-server-*` (Coming Soon)

## Security Notes 
* Postgres: The POSTGRES_READ_ONLY flag is a software check. For true security, ensure the database user provided in DATABASE_URL has GRANT SELECT permissions only. 
* FileSystem: The server prevents directory traversal (../) outside the FILESYSTEM_ROOT.