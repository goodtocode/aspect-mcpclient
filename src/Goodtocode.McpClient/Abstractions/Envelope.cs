using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Goodtocode.Mcp
{
    /// <summary>
    /// Transport-agnostic envelope for all MCP operations.
    /// Carries either a Result or a Problem (never both).
    /// </summary>
    public class Envelope<T>
    {
        public string Operation { get; set; }
        public string CorrelationId { get; set; }
        public DateTimeOffset SentUtc { get; set; }
        public T? Result { get; set; }
        public Problem? Problem { get; set; }
        public Continuation? Continue { get; set; }
        public IReadOnlyDictionary<string, string>? Metadata { get; set; }

        [JsonIgnore]
        public bool HasProblem => Problem != null;

        [JsonIgnore]
        public bool HasResult => Result != null && Problem == null;

        public Envelope(string operation, string correlationId, DateTimeOffset sentUtc, T? result = default, Problem? problem = null, Continuation? cont = null, IReadOnlyDictionary<string, string>? metadata = null)
        {
            Operation = operation;
            CorrelationId = correlationId;
            SentUtc = sentUtc;
            Result = result;
            Problem = problem;
            Continue = cont;
            Metadata = metadata;
        }
    }

    public class Problem
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string? Target { get; set; }
        public IReadOnlyDictionary<string, string[]>? Details { get; set; }

        public Problem(string code, string message, string? target = null, IReadOnlyDictionary<string, string[]>? details = null)
        {
            Code = code;
            Message = message;
            Target = target;
            Details = details;
        }
    }

    public class Continuation
    {
        public string Token { get; set; }
        public int? NextPageSize { get; set; }

        public Continuation(string token, int? nextPageSize = null)
        {
            Token = token;
            NextPageSize = nextPageSize;
        }
    }

    public class PageResult<TItem>
    {
        public IReadOnlyList<TItem> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long? TotalCount { get; set; }

        public PageResult(IReadOnlyList<TItem> items, int pageNumber, int pageSize, long? totalCount = null)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }

    public class BatchItem<TItem>
    {
        public TItem? Result { get; set; }
        public Problem? Problem { get; set; }
        public string? Key { get; set; }

        public BatchItem(TItem? result, Problem? problem = null, string? key = null)
        {
            Result = result;
            Problem = problem;
            Key = key;
        }
    }
}
