# OpenMcp

[![CI](https://github.com/rahulanand428/OpenMcp/actions/workflows/ci.yml/badge.svg)](https://github.com/rahulanand428/OpenMcp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/OpenMcp.Client.svg)](https://www.nuget.org/packages/OpenMcp.Client)
[![Docker Pulls](https://img.shields.io/docker/pulls/openmcpserver/mcp-postgres)](https://hub.docker.com/u/openmcpserver)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)

**OpenMcp** is an open-source, enterprise-grade implementation of the **Model Context Protocol (MCP)**. It provides Dockerized MCP Servers and a high-performance .NET Client SDK to easily connect AI Agents (like Semantic Kernel) to your data.

This project decouples your AI agent's tools from its core logic, allowing you to run sandboxed, secure, and independently scalable tools as microservices.

##  Features

*   **Generic .NET SDK:** `OpenMcp.Client` works with any MCP-compliant server and provides a high-level API for developers.
*   **Dockerized Servers:** Pre-built Python servers for PostgreSQL, FileSystem, and Web Search that you can run with a single command.
*   **Semantic Kernel Ready:** The .NET client is designed as a plug-and-play plugin for Microsoft Semantic Kernel, instantly giving your agents new capabilities.
*   **Secure & Sandboxed:** The servers are designed with security in mind, featuring read-only database modes and a "jailed" file system to prevent unauthorized access.
*   **Language Agnostic:** While the SDK is for .NET, the Dockerized MCP servers can be consumed by clients written in any language (Python, Go, TypeScript, etc.).

##  Quick Start

This guide is split into two paths depending on your use case.

---

### Path A: For .NET Developers (Using the SDK)

Follow this path if you are building a .NET application or an AI agent with Semantic Kernel and want to use the pre-built tools.

#### Step 1: Run the MCP Servers

The easiest way to run the pre-built MCP servers is with Docker Compose. You only need one file to get started.

```bash
# 1. Download the Docker Compose file for consumers.
# You can save the raw content from this URL:
 https://raw.githubusercontent.com/rahulanand428/OpenMcp/main/docker-compose.hub.yml

# 2. (Optional) Create a .env file in the same directory to customize ports or other settings.
# You can use the .env.example file in this repository as a template.

# 3. Start all servers.
# This command pulls the latest images from Docker Hub and runs them.
docker compose -f docker-compose.hub.yml up -d
```

This will start the PostgreSQL, FileSystem, and DuckDuckGo MCP servers on your machine.

> **Note for Contributors:** If you plan to modify the Python server code, you should clone the full repository and use the development `docker-compose.yml` file instead. This file builds images from your local source code, allowing you to test your changes.
> ```bash
> # For contributors only
> git clone https://github.com/rahulanand428/OpenMcp.git
> cd OpenMcp
> docker compose up --build -d
> ```

#### Step 2: Install the NuGet Package

In your .NET project, install the client SDK from NuGet.

```bash
dotnet add package OpenMcp.Client
```

#### Step 3: Use the Client in Your Application

You can now instantiate the clients and use them directly or import them as plugins into Semantic Kernel.

```csharp
// See the full example in: /samples/dotnet/ConsoleClient/Program.cs

using Microsoft.SemanticKernel;
using OpenMcp.Client.Plugins;

// 1. Setup HttpClient and Logger
var httpClient = new HttpClient();
var logger = /* your ILogger instance */;

// 2. Initialize the MCP Client for a specific server
var ddgClient = new DuckDuckGoMcpClient(httpClient, "http://localhost:8083", logger);

// 3. Import into Semantic Kernel
var kernel = Kernel.CreateBuilder().Build();
kernel.Plugins.AddFromObject(ddgClient, "DuckDuckGo");

// 4. Let the agent use the tool!
var result = await kernel.InvokeAsync("DuckDuckGo", "Search", new() 
{
    ["query"] = "Latest news on .NET 9" 
});

Console.WriteLine(result);
```

---

### Path B: For Docker & Other Language Users

Follow this path if you only want to run the secure, sandboxed tool servers and connect to them from a non-.NET application (e.g., a Python agent using LangChain).

#### Step 1: Run the MCP Servers

You can run the pre-built images directly from Docker Hub.

```bash
# 1. Download the consumer-focused Docker Compose file:
# https://raw.githubusercontent.com/rahulanand428/OpenMcp/main/docker-compose.hub.yml

# 2. Start the services
docker compose -f docker-compose.hub.yml up -d
```

The servers are now running and accessible at the following default endpoints:
*   **PostgreSQL:** `http://localhost:8081`
*   **FileSystem:** `http://localhost:8082`
*   **DuckDuckGo:** `http://localhost:8083`

#### Step 2: Connect from Your Application

You can connect to the `/sse` endpoint of any server from any language that supports Server-Sent Events. This will establish a session and provide a `session_id`.

You can test connectivity with `curl`:
```bash
# You should see a "session_id=..." message
curl -N http://localhost:8083/sse
```

Once you have a session, you can communicate with the server by sending `POST` requests to the `/messages` endpoint, following the JSON-RPC 2.0 and MCP specifications.

##  Configuration

You can customize the Docker Compose setup by creating a `.env` file in the root of the project (copy from `.env.example`). This allows you to change host ports, database connection strings, and other server settings without modifying the `docker-compose.yml` file.

##  Repository Structure

*   `sdk/dotnet/`: The C# Client SDK source code.
    *   **SDK README** - Detailed guide for the NuGet package.
*   `src/`: Source code for the Python MCP Servers (Postgres, FileSystem, DuckDuckGo).
    *   **Servers README** - Detailed guide for the Docker images.
*   `samples/`: Example .NET Console Application demonstrating SDK usage.
    *   **Sample Code**
*   `tests/`: Unit and Integration tests.

##  Contributing

We welcome all contributions! Whether you're fixing a bug, adding a new feature, or improving documentation, your help is appreciated. Please read our **Contributing Guide** to get started.

This project has adopted the Contributor Covenant **Code of Conduct**.

##  Security

Security is a top priority. If you discover a vulnerability, please follow the instructions in our **Security Policy** to report it privately.

##  License

This project is licensed under the **MIT License**.

## 🔗 Useful Links

*   **GitHub Repository:** [https://github.com/rahulanand428/OpenMcp](https://github.com/rahulanand428/OpenMcp)
*   **NuGet Package:** [OpenMcp.Client](https://www.nuget.org/packages/OpenMcp.Client)
*   **Docker Hub:** [openmcpserver](https://hub.docker.com/u/openmcpserver)
*   **Documentation:**
    *   [Client SDK Guide](https://github.com/rahulanand428/OpenMcp/blob/main/sdk/dotnet/OpenMcp.Client/Readme.md)
    *   [MCP Servers Guide](https://github.com/rahulanand428/OpenMcp/blob/main/src/README.md)
*   **Samples:**
    *   [.NET Console Client](https://github.com/rahulanand428/OpenMcp/tree/main/samples/dotnet/ConsoleClient)
