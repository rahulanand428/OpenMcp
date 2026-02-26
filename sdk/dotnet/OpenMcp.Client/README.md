# OpenMcp.Client

[![NuGet](https://img.shields.io/nuget/v/OpenMcp.Client.svg)](https://www.nuget.org/packages/OpenMcp.Client)

A generic, high-performance .NET client for the **Model Context Protocol (MCP)**.  
Designed to integrate MCP Servers (like PostgreSQL, FileSystem, Web Search) into **Semantic Kernel** agents or standard .NET applications.

## Installation

```bash
dotnet add package OpenMcp.Client
```

##  Usage with Semantic Kernel

This is the primary use case. The client is designed to be imported directly as a Plugin.

```csharp
using Microsoft.SemanticKernel;
using OpenMcp.Client.Plugins;

// 1. Setup Dependency Injection (HttpClient is required)
var httpClient = new HttpClient();
var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DuckDuckGoMcpClient>();

// 2. Initialize the MCP Client
var ddgClient = new DuckDuckGoMcpClient(httpClient, "http://localhost:8083", logger);

// 3. Import into Kernel
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.Plugins.AddFromObject(ddgClient, "DuckDuckGo");
var kernel = kernelBuilder.Build();

// 4. The Agent can now use the tool automatically!
var result = await kernel.InvokeAsync("DuckDuckGo", "Search", 
    new KernelArguments { ["query"] = "Latest .NET 9 features" });
```

## 🛠 Available Clients

### 1. DuckDuckGoMcpClient
*   **Purpose:** Web Search (Privacy focused).
*   **Tools:** `SearchAsync(query, maxResults)`

### 2. FileSystemMcpClient
*   **Purpose:** Read/Write files in a sandboxed environment.
*   **Tools:** `ListDirectoryAsync`, `ReadFileAsync`, `WriteFileAsync`

### 3. PostgresMcpClient
*   **Purpose:** Query databases safely.
*   **Tools:** `QueryDatabaseAsync`, `FindTablesAsync`, `GetTableSchemaAsync`

## Manual Usage

You can also use the clients directly without Semantic Kernel.

```csharp
using OpenMcp.Client.Plugins;

var client = new FileSystemMcpClient(httpClient, "http://localhost:8082", logger);

// Direct call
await client.WriteFileAsync("notes.txt", "Meeting at 10 AM");
var content = await client.ReadFileAsync("notes.txt");
```

## Configuration

The clients require a `BaseUrl` pointing to the running MCP Server (usually a Docker container).

*   **Postgres:** `http://localhost:8081`
*   **FileSystem:** `http://localhost:8082`
*   **DuckDuckGo:** `http://localhost:8083`


## MCP server docker images
To pull and start MCP servers from docker refer to doc -   https://github.com/rahulanand428/OpenMcp/blob/9b322daea2c92a7c18b28c8736405a9a9babb07b/src/README.md

*   **GitHub Repository:** [https://github.com/rahulanand428/OpenMcp](https://github.com/rahulanand428/OpenMcp)
*   **NuGet Package:** [OpenMcp.Client](https://www.nuget.org/packages/OpenMcp.Client)
*   **Docker Hub:** [openmcpserver](https://hub.docker.com/u/openmcpserver)
*   **Documentation:**
    *   [Client SDK Guide](https://github.com/rahulanand428/OpenMcp/blob/main/sdk/dotnet/OpenMcp.Client/Readme.md)
    *   [MCP Servers Guide](https://github.com/rahulanand428/OpenMcp/blob/main/src/README.md)
*   **Samples:**
    *   [.NET Console Client](https://github.com/rahulanand428/OpenMcp/tree/main/samples/dotnet/ConsoleClient)