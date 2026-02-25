using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace StockBix.Mcp.Client.Plugins
{
    public class StockDataPlugin : McpClientBase
    {
        public StockDataPlugin(IHttpClientFactory clientFactory, ILogger<StockDataPlugin> logger) 
            : base(clientFactory.CreateClient(), "http://mcp-postgres:8080", logger)
        {
        }

        [KernelFunction, Description("Executes a SQL query to fetch stock or indices data.")]
        public async Task<string> QueryDatabaseAsync(
            [Description("The SQL query to execute. Read-only access.")] string query)
        {
            // The mcp-postgres server exposes a tool named 'query'
            return await CallToolAsync("query", new { sql = query });
        }

        [KernelFunction, Description("Gets historical price data for a symbol.")]
        public async Task<string> GetStockHistoryAsync(
            [Description("Stock Symbol (e.g. hcltech)")] string symbol,
            [Description("Limit rows")] int limit = 10)
        {
            // Enforce a reasonable max limit to prevent context exhaustion
            if (limit > 50) limit = 50;

            // Helper to construct SQL so the LLM doesn't have to guess schema every time
            string sql = $"SELECT * FROM {symbol} ORDER BY month DESC LIMIT {limit}";
            var result = await CallToolAsync("query", new { sql = sql });
            _logger.LogInformation("mcp-postgre tool result for {Symbol}: {Result}", symbol, result);
            return result;
        }

        [KernelFunction, Description("Finds tables in the database that match a specific name pattern. Use this to find the correct table name if the stock symbol doesn't work.")]
        public async Task<string> FindTablesAsync(
            [Description("The partial name to search for (e.g. 'shriram').")] string searchPattern)
        {
            // Basic sanitization to prevent breaking the SQL string
            var safePattern = searchPattern.Replace("'", "").Replace(";", "").Trim();
            string sql = $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name ILIKE '%{safePattern}%';";
            var result = await CallToolAsync("query", new { sql = sql });
            _logger.LogInformation("mcp-postgre find tables result for {Pattern}: {Result}", searchPattern, result);
            return result;
        }

        [KernelFunction, Description("Gets the column schema for a specific table.")]
        public async Task<string> GetTableSchemaAsync(
            [Description("The table name")] string tableName)
        {
            string sql = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{tableName}';";
            var result = await CallToolAsync("query", new { sql = sql });
            _logger.LogInformation("mcp-postgre schema result for {TableName}: {Result}", tableName, result);
            return result;
        }
    }
}