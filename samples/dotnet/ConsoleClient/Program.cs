using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenMcp.Client;
using OpenMcp.Client.Plugins;

// 1. Setup Configuration
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

IConfiguration config = builder.Build();

// 2. Setup DI and Logging
var serviceProvider = new ServiceCollection()
    .AddLogging(configure => configure.AddConsole())
    .AddSingleton<IConfiguration>(config)
    .AddHttpClient()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// await TestDuckDuckGoSearchAsync();
// await TestFileSystemAsync();
// await TestPostgresAsync();
await TestSemanticKernelAgentAsync();

logger.LogInformation("All tests complete. Done.");


async Task TestDuckDuckGoSearchAsync()
{
    string url = config["McpEndpoints:DuckDuckGo"] ?? "http://localhost:8083";
    logger.LogInformation($"\n[ConsoleClient] --- Testing DuckDuckGo ({url}) ---");
    try
    {
        using var client = new DuckDuckGoMcpClient(httpClientFactory.CreateClient(), url, logger);
        var result = await client.SearchAsync("Latest news on .NET 9");
        Console.WriteLine("\n--- Search Result ---");
        Console.WriteLine(result);
        Console.WriteLine("---------------------\n");
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
    var kernelBuilder = Kernel.CreateBuilder();
    // Note: In a real app, you would add .AddOpenAIChatCompletion(...) here.
    // Since we don't have an LLM configured in this sample, we will just verify the plugin registration.
    var kernel = kernelBuilder.Build();

    // 2. Create the MCP Client
    string ddgUrl = config["McpEndpoints:DuckDuckGo"] ?? "http://localhost:8083";
    using var ddgClient = new DuckDuckGoMcpClient(httpClientFactory.CreateClient(), ddgUrl, logger);

    // 3. Import the MCP Client as a Plugin
    kernel.Plugins.AddFromObject(ddgClient, "DuckDuckGo");

    logger.LogInformation("Plugin 'DuckDuckGo' registered.");

    // 4. Debug: List all functions in the plugin to verify names
    var plugin = kernel.Plugins["DuckDuckGo"];
    foreach (var func in plugin)
    {
        logger.LogInformation($" - Available Function: {func.Name}");
    }

    // 5. Invoke it via the Kernel (Simulating what an Agent would do)
    // Note: We use the function name found in the plugin (usually matches method name 'SearchAsync')
    if (plugin.TryGetFunction("Search", out var function))
    {
        var result = await kernel.InvokeAsync(function, new KernelArguments { ["query"] = "Semantic Kernel 1.0 release date" });
        Console.WriteLine("\n--- Kernel Invocation Result ---");
        Console.WriteLine(result);
        Console.WriteLine("--------------------------------\n");
    }
    else
    {
        logger.LogError("Function 'SearchAsync' not found in plugin.");
    }
}
