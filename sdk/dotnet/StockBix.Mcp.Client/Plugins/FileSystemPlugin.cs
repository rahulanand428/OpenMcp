using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace StockBix.Mcp.Client.Plugins
{
    public class FileSystemPlugin : McpClientBase
    {
        public FileSystemPlugin(IHttpClientFactory clientFactory, ILogger<FileSystemPlugin> logger) 
            : base(clientFactory.CreateClient(), "http://mcp-filesystem:8080", logger)
        {
        }

        [KernelFunction, Description("Writes text to a file.")]
        public async Task<string> WriteFileAsync(
            [Description("The path to write to (relative to /data)")] string path,
            [Description("The content to write")] string content)
        {
            _logger.LogInformation($"[FileSystemPlugin] WriteFileAsync called with path: {path}, content length: {content?.Length ?? 0}");
            
            try
            {
                // mcp-filesystem exposes 'write_file'
                _logger.LogInformation($"[FileSystemPlugin] Calling CallToolAsync('write_file') for path: {path}");
                var result = await CallToolAsync("write_file", new { path = path, content = content });
                
                _logger.LogInformation($"[FileSystemPlugin] Tool result received: {result}");
                _logger.LogInformation("mcp-filesystem tool result for report {Path}: {Result}", path, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[FileSystemPlugin] Error calling write_file tool: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"[FileSystemPlugin] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}