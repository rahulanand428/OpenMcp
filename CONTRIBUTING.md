# Contributing to OpenMcp

Thank you for your interest in contributing to OpenMcp! We welcome contributions from the community to help make this the best Model Context Protocol SDK for .NET.

## 🛠️ Getting Started

### Prerequisites
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Python 3.10+](https://www.python.org/) (Optional, for modifying MCP servers)

### Building the Project

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/rahulanand428/OpenMcp.git
    cd OpenMcp
    ```

2.  **Start the MCP Servers (Docker):**
    This will spin up Postgres, FileSystem, and DuckDuckGo servers.
    ```bash
    docker compose up --build -d
    ```

3.  **Build the .NET Solution:**
    ```bash
    dotnet build
    ```

4.  **Run the Console Client:**
    Verify everything is working by running the sample client.
    ```bash
    cd samples/dotnet/ConsoleClient
    dotnet run
    ```

## 🧪 Running Tests

We use xUnit for testing.
```bash
dotnet test
```

## 📝 Pull Request Process
1.  Create a new branch for your feature or fix (`git checkout -b feature/amazing-feature`).
2.  Commit your changes.
3.  Push to the branch.
4.  Open a Pull Request targeting the `dev` branch.