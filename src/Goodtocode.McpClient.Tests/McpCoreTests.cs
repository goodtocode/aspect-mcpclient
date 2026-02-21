using Goodtocode.McpClient.Abstractions;
using Goodtocode.McpClient.Client;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Goodtocode.McpClient.Tests
{
    [TestClass]
    public class McpCoreTests
    {
        [TestMethod]
        public void EnvelopeSuccessHasResult()
        {
            // Arrange
            var envelope = McpEnvelope.Success("op", 42);
            // Act
            var hasResult = envelope.HasResult;
            // Assert
            Assert.IsTrue(hasResult);
            Assert.IsFalse(envelope.HasProblem);
            Assert.AreEqual(42, envelope.Result);
        }

        [TestMethod]
        public void EnvelopeFailureHasProblem()
        {
            // Arrange
            var problem = new Problem("code", "msg");
            var envelope = McpEnvelope.Failure<int>("op", problem);
            // Act
            var hasProblem = envelope.HasProblem;
            // Assert
            Assert.IsTrue(hasProblem);
            Assert.IsFalse(envelope.HasResult);
            Assert.AreEqual(problem, envelope.Problem);
        }

        [TestMethod]
        public void ContinuationRecordProperties()
        {
            // Arrange
            var cont = new Continuation("token", 10);
            // Act & Assert
            Assert.AreEqual("token", cont.Token);
            Assert.AreEqual(10, cont.NextPageSize);
        }

        [TestMethod]
        public void PageResultRecordProperties()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var page = new PageResult<int>(items, 1, 3, 100);
            // Act & Assert
            CollectionAssert.AreEqual(items, (System.Collections.ICollection)page.Items);
            Assert.AreEqual(1, page.PageNumber);
            Assert.AreEqual(3, page.PageSize);
            Assert.AreEqual(100, page.TotalCount);
        }

        [TestMethod]
        public void BatchItemRecordProperties()
        {
            // Arrange
            var batch = new BatchItem<int>(42, new Problem("code", "msg"), "key");
            // Act & Assert
            Assert.AreEqual(42, batch.Result);
            Assert.AreEqual("key", batch.Key);
            Assert.AreEqual("code", batch.Problem?.Code);
        }

        [TestMethod]
        public void SystemTextJsonMcpSerializerSerializeDeserialize()
        {
            // Arrange
            var serializer = new SystemTextJsonMcpSerializer();
            var envelope = McpEnvelope.Success("op", 42);
            // Act
            var json = serializer.Serialize(envelope);
            var deserialized = serializer.Deserialize<Envelope<int>>(json);
            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(envelope.Operation, deserialized.Operation);
            Assert.AreEqual(envelope.Result, deserialized.Result);
        }

        [TestMethod]
        public async Task SystemTextJsonMcpSerializerReadFromHttpAsync()
        {
            // Arrange
            var serializer = new SystemTextJsonMcpSerializer();
            var envelope = McpEnvelope.Success("op", 42);
            var content = new StringContent(serializer.Serialize(envelope), Encoding.UTF8, serializer.ContentType);
            // Act
            var result = await serializer.ReadFromHttpAsync<Envelope<int>>(content, CancellationToken.None);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.Result);
        }

        [TestMethod]
        public void McpCorrelationIdGetReturnsValue()
        {
            // Arrange & Act
            var id = McpCorrelationId.Get();
            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(id));
        }

        [TestMethod]
        public void McpTransportExceptionProperties()
        {
            // Arrange
            var problem = new Problem("code", "msg");
            var ex = new McpTransportException(problem, HttpStatusCode.BadRequest);
            // Act & Assert
            Assert.AreEqual(problem, ex.Problem);
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
        }

        [TestMethod]
        public void McpApplicationExceptionProperties()
        {
            // Arrange
            var problem = new Problem("code", "msg");
            var ex = new McpApplicationException(problem, HttpStatusCode.BadRequest);
            // Act & Assert
            Assert.AreEqual(problem, ex.Problem);
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
        }

        [TestMethod]
        public async Task McpHttpClientSendAsyncReturnsEnvelope()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var mcpClient = new McpHttpClient(httpClient);
            var request = 123;
            // Act
            var envelope = await mcpClient.SendAsync<int, int>(operation: "op", path: "http://test", request: request, options: null, ct: CancellationToken.None);
            // Assert
            Assert.IsNotNull(envelope);
            Assert.AreEqual(456, envelope.Result);
        }

        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // Fake handler for McpHttpClient test
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
    }
}
