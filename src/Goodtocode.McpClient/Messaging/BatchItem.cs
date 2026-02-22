namespace Goodtocode.McpClient.Messaging;

public class BatchItem<TItem>(TItem? result, Problem? problem = null, string? key = null)
{
    public TItem? Result { get; set; } = result;
    public Problem? Problem { get; set; } = problem;
    public string? Key { get; set; } = key;
}
