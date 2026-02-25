using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using StockBix.Mcp.Client;

namespace StockBix.Mcp.Client.Plugins
{
    /// <summary>
    /// MCP Client for the DuckDuckGo Search Server.
    /// Can be imported directly into Semantic Kernel as a Plugin.
    /// </summary>
    public class DuckDuckGoMcpClient : McpClientBase
    {
        public DuckDuckGoMcpClient(HttpClient httpClient, string baseUrl, ILogger logger) 
            : base(httpClient, baseUrl, logger)
        {
        }

        [KernelFunction, Description("Searches the web for real-time information.")]
        public async Task<string> SearchAsync(
            [Description("The search query")] string query)
        {
            _logger.LogInformation($"[DuckDuckGoMcpClient] SearchAsync called with query: {query}");
            
            try
            {
                // mcp-duckduckgo exposes 'search' tool
                var result = await CallToolAsync("search", new { query = query });
                
                _logger.LogInformation($"[DuckDuckGoMcpClient] Search success. Result length: {result?.Length ?? 0}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DuckDuckGoMcpClient] Error: {ex.Message}");
                throw;
            }
        }
    }
}