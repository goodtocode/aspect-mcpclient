using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Goodtocode.McpClient.Abstractions
{
    /// <summary>
    /// Transport-agnostic envelope for all MCP operations.
    /// Carries either a Result or a Problem (never both).
    /// </summary>
    public class Envelope<T>(string operation, string correlationId, DateTimeOffset sentUtc, T? result = default, Problem? problem = null, Continuation? cont = null, IReadOnlyDictionary<string, string>? metadata = null)
    {
        public string Operation { get; set; } = operation;
        public string CorrelationId { get; set; } = correlationId;
        public DateTimeOffset SentUtc { get; set; } = sentUtc;
        public T? Result { get; set; } = result;
        public Problem? Problem { get; set; } = problem;
        public Continuation? Continue { get; set; } = cont;
        public IReadOnlyDictionary<string, string>? Metadata { get; set; } = metadata;

        [JsonIgnore]
        public bool HasProblem => Problem != null;

        [JsonIgnore]
        public bool HasResult => Result != null && Problem == null;
    }

    public class Problem(string code, string message, string? target = null, IReadOnlyDictionary<string, string[]>? details = null)
    {
        public string Code { get; set; } = code;
        public string Message { get; set; } = message;
        public string? Target { get; set; } = target;
        public IReadOnlyDictionary<string, string[]>? Details { get; set; } = details;
    }

    public class Continuation(string token, int? nextPageSize = null)
    {
        public string Token { get; set; } = token;
        public int? NextPageSize { get; set; } = nextPageSize;
    }

    public class PageResult<TItem>(IReadOnlyList<TItem> items, int pageNumber, int pageSize, long? totalCount = null)
    {
        public IReadOnlyList<TItem> Items { get; set; } = items;
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public long? TotalCount { get; set; } = totalCount;
    }

    public class BatchItem<TItem>(TItem? result, Problem? problem = null, string? key = null)
    {
        public TItem? Result { get; set; } = result;
        public Problem? Problem { get; set; } = problem;
        public string? Key { get; set; } = key;
    }
}
