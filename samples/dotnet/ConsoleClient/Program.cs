using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenMcp.Client;
using OpenMcp.Client.Plugins;

// 1. Setup Configuration
// Loads settings from appsettings.json and environment variables (useful for Docker/Cloud overrides)
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

IConfiguration config = builder.Build();

// 2. Setup DI and Logging
// MCP Clients rely on HttpClient for SSE (Server-Sent Events) and HTTP POST operations.
var serviceProvider = new ServiceCollection()
    .AddLogging(configure => configure.AddConsole())
    .AddSingleton<IConfiguration>(config)
    .AddHttpClient()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// 3. Execute Tests
// Uncomment specific tests to verify individual MCP servers manually.

//await TestDuckDuckGoSearchAsync();

// await TestFileSystemAsync();

// await TestPostgresAsync();

// By default, we test the Semantic Kernel integration which demonstrates the "Agentic" capability.
await TestSemanticKernelAgentAsync();

logger.LogInformation("All tests complete. Done.");

async Task TestDuckDuckGoSearchAsync()
{
    // Example: Manually instantiating an MCP Client without Semantic Kernel.
    // Useful for direct tool usage in standard .NET apps.
    string url = config["McpEndpoints:DuckDuckGo"] ?? "http://localhost:8083";
    logger.LogInformation($"\n[ConsoleClient] --- Testing DuckDuckGo ({url}) ---");
    try
    {
        using var client = new DuckDuckGoMcpClient(httpClientFactory.CreateClient(), url, logger);
        var result = await client.SearchAsync("Latest news on .NET 9",11);
        Console.WriteLine("\n--- Search Result ---");
        Console.WriteLine(result);
        Console.WriteLine("---------------------\n");

        // 2. Test News Search
        var newsResult = await client.SearchNewsAsync("Artificial Intelligence trends",5);
        Console.WriteLine("\n--- News Search Result ---");
        Console.WriteLine(newsResult);

        // 3. Test Site Specific Search
        var siteResult = await client.SearchSiteAsync("microsoft.com", "semantic kernel",6);
        Console.WriteLine("\n--- Site Search Result (microsoft.com) ---");
        Console.WriteLine(siteResult);

        // 4. Test Fetch Page (Content Extraction)
        var pageContent = await client.FetchPageAsync("https://wikipedia.com");
        Console.WriteLine("\n--- Fetch Page Result ---");
        Console.WriteLine(pageContent);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to execute DuckDuckGo MCP tool");
    }
}

async Task TestFileSystemAsync()
{
    string url = config["McpEndpoints:FileSystem"] ?? "http://localhost:8082";
    logger.LogInformation($"\n[ConsoleClient] --- Testing FileSystem ({url}) ---");
    try
    {
        using var client = new FileSystemMcpClient(httpClientFactory.CreateClient(), url, logger);
        await client.WriteFileAsync("test.txt", $"Hello from the Console Client at {DateTime.UtcNow}!");
        var result = await client.ListDirectoryAsync(".");
        Console.WriteLine("\n--- List Directory Result ---");
        Console.WriteLine(result);
        Console.WriteLine("---------------------------\n");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to execute FileSystem MCP tool");
    }
}

async Task TestPostgresAsync()
{
    string url = config["McpEndpoints:Postgres"] ?? "http://localhost:8081";
    logger.LogInformation($"\n[ConsoleClient] --- Testing Postgres ({url}) ---");
    try
    {
        using var client = new PostgresMcpClient(httpClientFactory.CreateClient(), url, logger);
        var result = await client.FindTablesAsync("%"); // Find all tables
        Console.WriteLine("\n--- Find Tables Result ---");
        Console.WriteLine(result);
        Console.WriteLine("--------------------------\n");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to execute Postgres MCP tool");
    }
}

async Task TestSemanticKernelAgentAsync()
{
    logger.LogInformation("\n[ConsoleClient] --- Testing Semantic Kernel Agent Integration ---");
    
    // 1. Create the Kernel
    // The Kernel is the core of the AI Agent. It manages plugins and AI services.
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Note: In a real-world scenario, you would add an LLM service here:
    // kernelBuilder.AddOpenAIChatCompletion("gpt-4", "your-api-key");
    var kernel = kernelBuilder.Build();

    // 2. Create the MCP Client
    // We instantiate the MCP client which connects to the running Docker container.
    string ddgUrl = config["McpEndpoints:DuckDuckGo"] ?? "http://localhost:8083";
    using var ddgClient = new DuckDuckGoMcpClient(httpClientFactory.CreateClient(), ddgUrl, logger);

    // 3. Import the MCP Client as a Plugin
    // This is the key step: The MCP Client is registered as a "Plugin" in Semantic Kernel.
    // The Kernel scans the client for methods decorated with [KernelFunction].
    kernel.Plugins.AddFromObject(ddgClient, "DuckDuckGo");

    logger.LogInformation("Plugin 'DuckDuckGo' registered.");

    // 4. Debug: Verify Function Registration
    // It's good practice to list available functions to ensure naming conventions are correct.
    // Semantic Kernel often strips "Async" suffixes from function names.
    var plugin = kernel.Plugins["DuckDuckGo"];
    foreach (var func in plugin)
    {
        logger.LogInformation($" - Available Function: {func.Name}");
    }

    // 5. Invoke it via the Kernel (Simulating what an Agent would do)
    // Instead of calling client.SearchAsync() directly, we ask the Kernel to invoke it.
    // In a full Agent scenario, the LLM would automatically select this tool and generate the arguments.
    if (plugin.TryGetFunction("Search", out var function))
    {
        // We pass arguments via KernelArguments. 
        // Note: 'maxResults' matches the parameter name in the C# method.
        var result = await kernel.InvokeAsync(function, new KernelArguments 
        { 
            ["query"] = "Semantic Kernel 1.0 release date",
            ["maxResults"] = 20 
        });
        Console.WriteLine("\n--- Kernel Invocation Result ---");
        Console.WriteLine(result);
        Console.WriteLine("--------------------------------\n");
    }
    else
    {
        logger.LogError("Function 'Search' not found in plugin.");
    }
}
