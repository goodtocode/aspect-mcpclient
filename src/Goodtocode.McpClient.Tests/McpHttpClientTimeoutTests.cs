using Goodtocode.McpClient.Abstractions;
using Goodtocode.McpClient.Client;
using System.Net;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class McpHttpClientTimeoutTests
{
    [TestMethod]
    public async Task SendAsyncReturnsTimeoutEnvelopeWhenTimeoutOccurs()
    {
        var handler = new TimeoutHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var mcpClient = new McpHttpClient(httpClient);

        var options = new McpSendOptions { Timeout = TimeSpan.FromMilliseconds(10) };
        var envelope = await mcpClient.SendAsync<int, int>("op", "http://test", 123, options, TestContext.CancellationToken);

        Assert.IsNotNull(envelope);
        Assert.IsTrue(envelope.HasProblem);
        Assert.AreEqual("Timeout", envelope.Problem?.Code);
    }

    private class TimeoutHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Thread.Sleep(100); // Simulate timeout
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    public TestContext TestContext { get; set; }
}