using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Goodtocode.McpClient.Abstractions;

public static class McpEnvelope
{
    public static Envelope<T> Success<T>(string operation, T result, string? correlationId = null,
        Continuation? cont = null, IReadOnlyDictionary<string, string>? metadata = null)
        => new(operation, correlationId ?? McpCorrelationId.Get(), DateTimeOffset.UtcNow, result, null, cont, metadata);

    public static Envelope<T> Failure<T>(string operation, Problem problem, string? correlationId = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new(operation, correlationId ?? McpCorrelationId.Get(), DateTimeOffset.UtcNow, default, problem, null, metadata);
}

public static class McpCorrelationId
{
    public static string Get() => Activity.Current?.Id ?? Guid.NewGuid().ToString("n");
}

public static class McpIdempotency
{
    public static string New() => Guid.NewGuid().ToString("n");
    public static string FromDeterministic(params string[] parts)
        => string.Join("-", parts).ToLowerInvariant();
}
