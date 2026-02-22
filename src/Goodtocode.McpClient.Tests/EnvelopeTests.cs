using Goodtocode.McpClient.Messaging;

namespace Goodtocode.McpClient.Tests;

[TestClass]
public class EnvelopeTests
{
    [TestMethod]
    public void EnvelopeSuccessHasResult()
    {
        var envelope = McpEnvelope.Success("op", 42);
        Assert.IsTrue(envelope.HasResult);
        Assert.IsFalse(envelope.HasProblem);
        Assert.AreEqual(42, envelope.Result);
    }

    [TestMethod]
    public void EnvelopeFailureHasProblem()
    {
        var problem = new Problem("code", "msg");
        var envelope = McpEnvelope.Failure<int>("op", problem);
        Assert.IsTrue(envelope.HasProblem);
        Assert.IsFalse(envelope.HasResult);
        Assert.AreEqual(problem, envelope.Problem);
    }

    [TestMethod]
    public void ContinuationRecordProperties()
    {
        var cont = new Continuation("token", 10);
        Assert.AreEqual("token", cont.Token);
        Assert.AreEqual(10, cont.NextPageSize);
    }

    [TestMethod]
    public void PageResultRecordProperties()
    {
        var items = new List<int> { 1, 2, 3 };
        var page = new PageResult<int>(items, 1, 3, 100);
        CollectionAssert.AreEqual(items, (System.Collections.ICollection)page.Items);
        Assert.AreEqual(1, page.PageNumber);
        Assert.AreEqual(3, page.PageSize);
        Assert.AreEqual(100, page.TotalCount);
    }

    [TestMethod]
    public void BatchItemRecordProperties()
    {
        var batch = new BatchItem<int>(42, new Problem("code", "msg"), "key");
        Assert.AreEqual(42, batch.Result);
        Assert.AreEqual("key", batch.Key);
        Assert.AreEqual("code", batch.Problem?.Code);
    }
}