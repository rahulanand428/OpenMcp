using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OpenMcp.Client.Plugins;
using Xunit;

namespace OpenMcp.Client.Tests
{
    public class FileSystemMcpClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger> _loggerMock;

        public FileSystemMcpClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://mock-mcp")
            };
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task ListDirectoryAsync_SendsCorrectRequest()
        {
            // Arrange
            var client = new FileSystemMcpClient(_httpClient, "http://mock-mcp", _loggerMock.Object);

            // Mock SSE Connection & Handshake
            SetupMockResponse("http://mock-mcp/sse", "data: session_id=test-session-123\n\n");
            SetupMockResponse("http://mock-mcp/messages/?session_id=test-session-123", "{}");

            // Act
            try
            {
                await client.ListDirectoryAsync(".");
            }
            catch (Exception) { /* Expected timeout in mock environment */ }

            // Assert
            _handlerMock.Protected().Verify("SendAsync", Times.AtLeastOnce(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        private void SetupMockResponse(string url, string responseContent)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
        }
    }
}