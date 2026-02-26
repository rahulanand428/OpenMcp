using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using OpenMcp.Client;

namespace OpenMcp.Client.Plugins
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
            [Description("The search query")] string query,
            [Description("Max results to return")] int maxResults = 10)
        {
            _logger.LogInformation($"[DuckDuckGoMcpClient] SearchAsync called with query: {query}, maxResults: {maxResults}");
            
            try
            {
                // mcp-duckduckgo exposes 'search' tool
                var result = await CallToolAsync("search", new { query = query, max_results = maxResults });
                
                _logger.LogInformation($"[DuckDuckGoMcpClient] Search success. Result length: {result?.Length ?? 0}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DuckDuckGoMcpClient] Error: {ex.Message}");
                throw;
            }
        }

        [KernelFunction, Description("Fetches the text content of a specific URL.")]
        public async Task<string> FetchPageAsync(
            [Description("The URL to fetch")] string url)
        {
            _logger.LogInformation($"[DuckDuckGoMcpClient] FetchPageAsync called with url: {url}");
            try
            {
                return await CallToolAsync("fetch_page", new { url = url });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DuckDuckGoMcpClient] Error: {ex.Message}");
                throw;
            }
        }

        [KernelFunction, Description("Searches for news articles.")]
        public async Task<string> SearchNewsAsync(
            [Description("The news search query")] string query,
            [Description("Max results to return")] int maxResults = 10)
        {
            _logger.LogInformation($"[DuckDuckGoMcpClient] SearchNewsAsync called with query: {query}");
            return await CallToolAsync("search_news", new { query = query, max_results = maxResults });
        }

        [KernelFunction, Description("Searches for pages within a specific website.")]
        public async Task<string> SearchSiteAsync(
            [Description("The domain to search (e.g., python.org)")] string domain,
            [Description("The search query")] string query,
            [Description("Max results to return")] int maxResults = 10)
        {
            _logger.LogInformation($"[DuckDuckGoMcpClient] SearchSiteAsync called with domain: {domain}, query: {query}");
            return await CallToolAsync("search_site", new { domain = domain, query = query, max_results = maxResults });
        }
    }
}