using System.Text.Json.Serialization;

namespace Goodtocode.McpClient.Abstractions;

/// <summary>
/// Transport-agnostic envelope for all MCP operations.
/// Carries either a Result or a Problem (never both).
/// </summary>

public class Envelope<T>(
    string operation,
    string correlationId,
    DateTimeOffset sentUtc,
    T? result = default,
    Problem? problem = null,
    Continuation? @continue = null,
    IReadOnlyDictionary<string, string>? metadata = null)
{
    public string Operation { get; set; } = operation;
    public string CorrelationId { get; set; } = correlationId;
    public DateTimeOffset SentUtc { get; set; } = sentUtc;
    public T? Result { get; set; } = result;
    public Problem? Problem { get; set; } = problem;
    public Continuation? Continue { get; set; } = @continue;
    public IReadOnlyDictionary<string, string>? Metadata { get; set; } = metadata;

    [JsonIgnore]
    public bool HasProblem => Problem != null;

    [JsonIgnore]
    public bool HasResult => Result != null && Problem == null;
}