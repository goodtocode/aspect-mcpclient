namespace Goodtocode.McpClient.Abstractions;

public class Problem(string code, string message, string? target = null, IReadOnlyDictionary<string, string[]>? details = null)
{
    public string Code { get; set; } = code;
    public string Message { get; set; } = message;
    public string? Target { get; set; } = target;
    public IReadOnlyDictionary<string, string[]>? Details { get; set; } = details;
}
