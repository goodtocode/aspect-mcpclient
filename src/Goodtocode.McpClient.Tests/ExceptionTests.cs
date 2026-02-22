using Goodtocode.McpClient.Client;
using Goodtocode.McpClient.Messaging;
using System.Net;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class ExceptionTests
{
    [TestMethod]
    public void McpTransportExceptionProperties()
    {
        var problem = new Problem("code", "msg");
        var ex = new McpTransportException(problem, HttpStatusCode.BadRequest);
        Assert.AreEqual(problem, ex.Problem);
        Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [TestMethod]
    public void McpApplicationExceptionProperties()
    {
        var problem = new Problem("code", "msg");
        var ex = new McpApplicationException(problem, HttpStatusCode.BadRequest);
        Assert.AreEqual(problem, ex.Problem);
        Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
    }
}