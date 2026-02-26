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
    /// Generic MCP Client for PostgreSQL Database interactions.
    /// </summary>
    public class PostgresMcpClient : McpClientBase
    {
        public PostgresMcpClient(HttpClient httpClient, string baseUrl, ILogger logger) 
            : base(httpClient, baseUrl, logger)
        {
        }

        [KernelFunction, Description("Executes a read-only SQL query against the database.")]
        public async Task<string> QueryDatabaseAsync(
            [Description("The SQL query to execute. Read-only access.")] string query)
        {
            _logger.LogInformation($"[PostgresMcpClient] Executing Query: {query}");
            // The mcp-postgres server exposes a tool named 'query'
            return await CallToolAsync("query", new { sql = query });
        }

        [KernelFunction, Description("Finds tables in the database that match a specific name pattern.")]
        public async Task<string> FindTablesAsync(
            [Description("The partial name to search for (e.g. 'shriram').")] string searchPattern)
        {
            // Basic sanitization to prevent breaking the SQL string
            var safePattern = searchPattern.Replace("'", "").Replace(";", "").Trim();
            string sql = $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name ILIKE '%{safePattern}%';";
            var result = await CallToolAsync("query", new { sql = sql });
            _logger.LogInformation("[PostgresMcpClient] Find tables result for {Pattern}: {Result}", searchPattern, result);
            return result;
        }

        [KernelFunction, Description("Gets the column schema for a specific table.")]
        public async Task<string> GetTableSchemaAsync(
            [Description("The table name")] string tableName)
        {   
            // Sanitize to prevent basic SQL injection in the table name.
            var safeTableName = tableName.Replace("'", "").Replace(";", "").Trim();
            string sql = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{safeTableName}';";
            var result = await CallToolAsync("query", new { sql = sql });
            _logger.LogInformation("[PostgresMcpClient] Schema result for {TableName}: {Result}", tableName, result);
            return result;
        }
    }
}