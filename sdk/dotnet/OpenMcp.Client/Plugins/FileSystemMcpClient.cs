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
    /// Generic MCP Client for File System operations.
    /// </summary>
    public class FileSystemMcpClient : McpClientBase
    {
        public FileSystemMcpClient(HttpClient httpClient, string baseUrl, ILogger logger) 
            : base(httpClient, baseUrl, logger)
        {
        }

        [KernelFunction, Description("Lists files and directories in the specified path.")]
        public async Task<string> ListDirectoryAsync(
            [Description("The path to list (relative to /data). Defaults to root.")] string path = ".")
        {
            _logger.LogInformation($"[FileSystemMcpClient] Listing directory: {path}");
            return await CallToolAsync("list_directory", new { path = path });
        }

        [KernelFunction, Description("Reads the content of a file.")]
        public async Task<string> ReadFileAsync(
            [Description("The path to the file (relative to /data)")] string path)
        {
            _logger.LogInformation($"[FileSystemMcpClient] Reading file: {path}");
            return await CallToolAsync("read_file", new { path = path });
        }

        [KernelFunction, Description("Writes text to a file.")]
        public async Task<string> WriteFileAsync(
            [Description("The path to write to (relative to /data)")] string path,
            [Description("The content to write")] string content)
        {
            _logger.LogInformation($"[FileSystemMcpClient] WriteFileAsync called with path: {path}, content length: {content?.Length ?? 0}");
            
            try
            {
                // mcp-filesystem exposes 'write_file'
                var result = await CallToolAsync("write_file", new { path = path, content = content });
                
                _logger.LogInformation($"[FileSystemMcpClient] Write success: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FileSystemMcpClient] Error writing file at path {Path}", path);
                throw;
            }
        }
    }
}