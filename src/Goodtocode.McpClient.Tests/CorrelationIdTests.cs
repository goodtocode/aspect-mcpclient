using Goodtocode.McpClient.Messaging;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class CorrelationIdTests
{
    [TestMethod]
    public void McpCorrelationIdGetReturnsValue()
    {
        var id = McpCorrelationId.Get();
        Assert.IsFalse(string.IsNullOrWhiteSpace(id));
    }
}