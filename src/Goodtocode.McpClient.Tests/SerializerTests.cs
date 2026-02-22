using Goodtocode.McpClient.Abstractions;
using Goodtocode.McpClient.Messaging;
using System.Text;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class SerializerTests
{
    [TestMethod]
    public void SystemTextJsonMcpSerializerSerializeDeserialize()
    {
        var serializer = new SystemTextJsonMcpSerializer();
        var envelope = McpEnvelope.Success("op", 42);
        var json = serializer.Serialize(envelope);
        var deserialized = serializer.Deserialize<Envelope<int>>(json);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(envelope.Operation, deserialized.Operation);
        Assert.AreEqual(envelope.Result, deserialized.Result);
    }

    [TestMethod]
    public async Task SystemTextJsonMcpSerializerReadFromHttpAsync()
    {
        var serializer = new SystemTextJsonMcpSerializer();
        var envelope = McpEnvelope.Success("op", 42);
        var content = new StringContent(serializer.Serialize(envelope), Encoding.UTF8, serializer.ContentType);
        var result = await serializer.ReadFromHttpAsync<Envelope<int>>(content, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.Result);
    }
}