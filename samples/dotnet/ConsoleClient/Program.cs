using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockBix.Mcp.Client;
using StockBix.Mcp.Client.Plugins;

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

// 3. Initialize MCP Client (Example: DuckDuckGo)
string ddgUrl = config["McpEndpoints:DuckDuckGo"] ?? "http://localhost:8083";
logger.LogInformation($"[ConsoleClient] Connecting to DuckDuckGo MCP at {ddgUrl}...");

try 
{
    // Instantiate the client with the URL from config
    using var client = new DuckDuckGoMcpClient(httpClientFactory.CreateClient(), ddgUrl, logger);

    // Execute the search
    var result = await client.SearchAsync("Stock market trends 2024");
    
    Console.WriteLine("\n--- Search Result ---");
    Console.WriteLine(result);
    Console.WriteLine("---------------------\n");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to execute MCP tool");
}

logger.LogInformation("Done.");
