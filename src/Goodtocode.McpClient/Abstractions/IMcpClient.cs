using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Goodtocode.Mcp
{
    public interface IMcpClient
    {
        Task<Envelope<TResponse>> SendAsync<TRequest, TResponse>(
            string operation,
            string path,
            TRequest request,
            McpSendOptions? options = null,
            CancellationToken ct = default);
    }

    public class McpSendOptions
    {
        public string? CorrelationId { get; set; }
        public string? IdempotencyKey { get; set; }
        public IReadOnlyDictionary<string, string>? ExtraHeaders { get; set; }
        public TimeSpan? Timeout { get; set; }
        public bool ThrowOnProblem { get; set; } = false;
    }
}
