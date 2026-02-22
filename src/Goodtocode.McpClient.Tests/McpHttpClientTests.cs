using Goodtocode.McpClient.Client;
using Goodtocode.McpClient.Messaging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class McpHttpClientTests
{
    [TestMethod]
    public async Task McpHttpClientSendAsyncReturnsEnvelope()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var mcpClient = new McpHttpClient(httpClient);
        var request = 123;
        var envelope = await mcpClient.SendAsync<int, int>("op", "http://test", request, ct: TestContext.CancellationToken);
        Assert.IsNotNull(envelope);
        Assert.AreEqual(456, envelope.Result);
    }

    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var envelope = McpEnvelope.Success("op", 456);
            var json = JsonSerializer.Serialize(envelope, CachedJsonOptions);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    public TestContext TestContext { get; set; }
}