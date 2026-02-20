using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Goodtocode.Mcp;

namespace Goodtocode.Mcp.Http
{
    public class McpHttpClient : IMcpClient
    {
        private readonly HttpClient _http;
        private readonly IMcpSerializer _serializer;

        public McpHttpClient(HttpClient http, IMcpSerializer? serializer = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _serializer = serializer ?? new SystemTextJsonMcpSerializer();
        }

        public async Task<Envelope<TResponse>> SendAsync<TRequest, TResponse>(
            string operation,
            string path,
            TRequest request,
            McpSendOptions? options = null,
            CancellationToken ct = default)
        {
            options ??= new McpSendOptions();

            using var cts = options.Timeout.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(ct)
                : null;
            if (cts is not null) cts.CancelAfter(options.Timeout.Value);
            var token = cts?.Token ?? ct;

            var correlationId = options.CorrelationId ?? McpCorrelationId.Get();

            using var httpReq = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = _serializer.ToHttpContent(request)
            };

            httpReq.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            if (!string.IsNullOrWhiteSpace(options.IdempotencyKey))
                httpReq.Headers.TryAddWithoutValidation("Idempotency-Key", options.IdempotencyKey);

            if (options.ExtraHeaders is not null)
            {
                foreach (var kvp in options.ExtraHeaders)
                    httpReq.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            HttpResponseMessage httpResp;
            try
            {
                httpResp = await _http.SendAsync(httpReq, HttpCompletionOption.ResponseHeadersRead, token)
                                      .ConfigureAwait(false);
            }
            catch (OperationCanceledException oce) when (!ct.IsCancellationRequested && options.Timeout.HasValue)
            {
                var timeoutProblem = new Problem("Timeout", $"Request timed out after {options.Timeout.Value.TotalMilliseconds:F0} ms.", null, null);
                var envTimeout = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, timeoutProblem);
                if (options.ThrowOnProblem) throw new McpTransportException(timeoutProblem, HttpStatusCode.RequestTimeout, oce);
                return envTimeout;
            }
            catch (Exception ex)
            {
                var transportProblem = new Problem("TransportError", ex.Message);
                var envTransport = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, transportProblem);
                if (options.ThrowOnProblem) throw new McpTransportException(transportProblem, null, ex);
                return envTransport;
            }

            Envelope<TResponse>? envelope = null;
            try
            {
                envelope = await _serializer.ReadFromHttpAsync<Envelope<TResponse>>(httpResp.Content, token)
                                            .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var parseProblem = new Problem("DeserializeError", $"Failed to parse response: {ex.Message}");
                envelope = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, parseProblem);
            }

            if (!httpResp.IsSuccessStatusCode && (envelope == null || !envelope.HasProblem))
            {
                var httpProblem = new Problem(httpResp.StatusCode.ToString(), $"HTTP {(int)httpResp.StatusCode} {httpResp.ReasonPhrase}");
                envelope = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, httpProblem);
            }

            if (options.ThrowOnProblem && envelope is { HasProblem: true })
                throw new McpApplicationException(envelope.Problem!, httpResp.StatusCode);

            return envelope!;
        }
    }

    public class McpTransportException : Exception
    {
        public Problem Problem { get; }
        public HttpStatusCode? StatusCode { get; }

        public McpTransportException(Problem problem, HttpStatusCode? statusCode = null, Exception? inner = null)
            : base(problem.Message, inner)
        {
            Problem = problem;
            StatusCode = statusCode;
        }
    }

    public class McpApplicationException : Exception
    {
        public Problem Problem { get; }
        public HttpStatusCode? StatusCode { get; }

        public McpApplicationException(Problem problem, HttpStatusCode? statusCode = null, Exception? inner = null)
            : base(problem.Message, inner)
        {
            Problem = problem;
            StatusCode = statusCode;
        }
    }
}
