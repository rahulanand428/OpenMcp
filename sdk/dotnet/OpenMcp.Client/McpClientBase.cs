using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMcp.Client
{
    /// <summary>
    /// Base class for interacting with MCP Servers via HTTP/SSE.
    /// </summary>
    public abstract class McpClientBase : IDisposable
    {
        protected readonly HttpClient HttpClient;
        protected readonly string BaseUrl;
        private string _sessionId;
        protected readonly ILogger _logger;
        private CancellationTokenSource _cts;
        private Task _sseTask;
        private bool _isInitialized = false;

        // Dictionary to map Request IDs to TaskCompletionSources for awaiting responses via SSE
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = new();

        protected McpClientBase(HttpClient httpClient, string baseUrl, ILogger logger)
        {
            HttpClient = httpClient;
            // FIX: Increase timeout for AI workloads (default is 100s)
            HttpClient.Timeout = TimeSpan.FromMinutes(5);
            BaseUrl = baseUrl.TrimEnd('/');
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Initializes the SSE connection to get a Session ID.
        /// </summary>
        protected async Task EnsureConnectedAsync()
        {
            if (!string.IsNullOrEmpty(_sessionId) && _isInitialized) return;

            // Start the SSE connection loop if not running
            if (_sseTask == null || _sseTask.IsCompleted)
            {
                _sseTask = ConnectToSseAsync(_cts.Token);
            }

            // Wait for session ID
            int retries = 0;
            while (string.IsNullOrEmpty(_sessionId) && retries < 50) // Wait up to 5 seconds
            {
                await Task.Delay(100);
                retries++;
            }

            if (string.IsNullOrEmpty(_sessionId))
            {
                throw new Exception($"Failed to obtain session ID from MCP server at {BaseUrl}");
            }

            if (!_isInitialized)
            {
                await PerformHandshakeAsync();
                _isInitialized = true;
            }
        }

        private async Task PerformHandshakeAsync()
        {
            _logger.LogInformation("[MCP] Performing initialization handshake...");
            
            // 1. Send Initialize Request
            var initParams = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "OpenMcpClient", version = "1.0" }
            };

            // We await the response to ensure the server is ready
            await SendRequestAsync("initialize", initParams);

            // 2. Send Initialized Notification
            await SendNotificationAsync("notifications/initialized");
            
            _logger.LogInformation("[MCP] Handshake complete.");
        }

        private async Task ConnectToSseAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"[MCP] Connecting to SSE endpoint: {BaseUrl}/sse");
                    
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/sse");
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
                    // request.Headers.Host = "localhost:8080"; // Removed hardcoded host for generic usage

                    // Use HttpCompletionOption.ResponseHeadersRead to get the stream immediately
                    using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var reader = new StreamReader(stream);

                    while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        if (line.StartsWith("data: "))
                        {
                            var data = line.Substring(6).Trim();
                            
                            if (data.Contains("session_id="))
                            {
                                var parts = data.Split(new[] { "session_id=" }, StringSplitOptions.None);
                                if (parts.Length > 1)
                                {
                                    var idPart = parts[1].Split('&')[0];
                                    if (_sessionId != idPart)
                                    {
                                        _sessionId = idPart;
                                        _logger.LogInformation($"[MCP] Session ID established: {_sessionId}");
                                    }
                                }
                            }
                            // Handle JSON-RPC Messages (Responses)
                            else if (data.StartsWith("{"))
                            {
                                try 
                                {
                                    using var doc = JsonDocument.Parse(data);
                                    var root = doc.RootElement.Clone(); // Clone to detach from stream
                                    
                                    if (root.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                                    {
                                        var id = idProp.GetString();
                                        if (_pendingRequests.TryRemove(id, out var tcs))
                                        {
                                            tcs.TrySetResult(root);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"[MCP] Error parsing SSE JSON: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    _logger.LogWarning($"[MCP] SSE connection error: {ex.Message}. Retrying in 5s...");
                    _sessionId = null; // Invalidate session
                    _isInitialized = false; // Require re-handshake
                    try 
                    { 
                        await Task.Delay(5000, cancellationToken); 
                    } 
                    catch (OperationCanceledException) { break; }
                }
            }
        }

        protected async Task<string> CallToolAsync(string toolName, object arguments)
        {
            _logger.LogInformation($"[MCP] Calling Tool: {toolName} | Args: {JsonSerializer.Serialize(arguments)}");
            await EnsureConnectedAsync();

            var requestParams = new
            {
                name = toolName,
                arguments = arguments
            };

            // Send request and wait for response via SSE
            var responseJson = await SendRequestAsync("tools/call", requestParams);
            
            if (responseJson.TryGetProperty("result", out var result))
            {
                _logger.LogInformation($"[MCP] Tool {toolName} Success.");
                return result.ToString();
            }
            
            if (responseJson.TryGetProperty("error", out var error))
            {
                 var errorMsg = error.ToString();
                 _logger.LogError($"[MCP] Tool {toolName} Error: {errorMsg}");
                 throw new Exception($"MCP Tool Error: {errorMsg}");
            }
            
            return responseJson.ToString();
        }

        private async Task<JsonElement> SendRequestAsync(string method, object parameters)
        {
            var id = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[id] = tcs;

            var requestPayload = new
            {
                jsonrpc = "2.0",
                id = id,
                method = method,
                @params = parameters
            };

            await PostMessageAsync(requestPayload);

            // Wait for response via SSE (with timeout)
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(60)));
            if (completedTask == tcs.Task)
            {
                return await tcs.Task;
            }
            else
            {
                _pendingRequests.TryRemove(id, out _);
                throw new TimeoutException($"MCP Request {method} timed out.");
            }
        }

        private async Task SendNotificationAsync(string method, object parameters = null)
        {
            var requestPayload = new
            {
                jsonrpc = "2.0",
                method = method,
                @params = parameters
            };
            await PostMessageAsync(requestPayload);
        }

        private async Task PostMessageAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            
            var uriBuilder = new UriBuilder($"{BaseUrl}/messages/");
            uriBuilder.Query = $"session_id={_sessionId}";
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri);
            // requestMessage.Headers.Host = "localhost:8080"; // Removed hardcoded host
            requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json"); 

            var response = await HttpClient.SendAsync(requestMessage);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"[MCP] Post Failed: {response.StatusCode} - {error}");
                throw new HttpRequestException($"MCP Post Failed: {error}");
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
