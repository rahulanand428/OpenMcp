# OpenMcp

[![CI](https://github.com/rahulanand428/OpenMcp/actions/workflows/ci.yml/badge.svg)](https://github.com/rahulanand428/OpenMcp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/OpenMcp.Client.svg)](https://www.nuget.org/packages/OpenMcp.Client)
[![Docker Pulls](https://img.shields.io/docker/pulls/openmcp/mcp-postgres)](https://hub.docker.com/u/openmcp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**OpenMcp** is an open-source, enterprise-grade implementation of the **Model Context Protocol (MCP)**. It provides Dockerized MCP Servers and a high-performance .NET Client SDK to easily connect AI Agents (like Semantic Kernel) to your data.

## 🌟 Features

*   **Generic .NET SDK:** `OpenMcp.Client` works with any MCP-compliant server.
*   **Dockerized Servers:** Pre-built Python servers for PostgreSQL, FileSystem, and Web Search.
*   **Semantic Kernel Ready:** Plug-and-play integration with Microsoft Semantic Kernel.
*   **Secure:** Read-only database modes and sandboxed file system access.

## 📦 Quick Start

### 1. Run the Servers
```bash
git clone https://github.com/rahulanand428/OpenMcp.git
cd OpenMcp
docker compose up --build -d
```

### 2. Install the SDK
```bash
dotnet add package OpenMcp.Client
```

### 3. Use in your Code
```csharp
using OpenMcp.Client.Plugins;

// Connect to the running Docker container
var client = new DuckDuckGoMcpClient(httpClient, "http://localhost:8083", logger);
var result = await client.SearchAsync("Open Source AI Tools");
```

## 📂 Repository Structure

*   `sdk/dotnet/`: The C# Client SDK source code.
*   `src/`: Source code for the Python MCP Servers (Postgres, FileSystem, DuckDuckGo).
*   `samples/`: Example Console Applications.
*   `tests/`: Unit and Integration tests.

## 🤝 Contributing

We welcome contributions! Please see CONTRIBUTING.md for details.
```