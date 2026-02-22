using Goodtocode.McpClient.Abstractions;
using Goodtocode.McpClient.Messaging;
using System.Net;
using System.Text.Json;

namespace Goodtocode.McpClient.Client;

public class McpHttpClient(HttpClient http, IMcpSerializer? serializer = null) : IMcpClient
{
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly IMcpSerializer _serializer = serializer ?? new SystemTextJsonMcpSerializer();

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
        if (options.Timeout.HasValue)
        {
            cts?.CancelAfter(options.Timeout.Value);
        }
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
        catch (HttpRequestException hre)
        {
            var transportProblem = new Problem("TransportError", "Network or HTTP error occurred. Please check your HttpClient setup, base address, authentication handler, and network connectivity.");
            var envTransport = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, transportProblem);
            if (options.ThrowOnProblem) throw new McpTransportException(transportProblem, null, hre);
            return envTransport;
        }
        catch (ArgumentException ae)
        {
            var transportProblem = new Problem("TransportError", "Invalid argument or misconfiguration detected. Please verify your HttpClient and request parameters.");
            var envTransport = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, transportProblem);
            if (options.ThrowOnProblem) throw new McpTransportException(transportProblem, null, ae);
            return envTransport;
        }
        catch (Exception ex)
        {
            var transportProblem = new Problem("TransportError", "An unexpected error occurred. Please check your HttpClient setup and network connectivity.");
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
        catch (TaskCanceledException)
        {
            // Check if the cancellation was due to a timeout
            var isTimeout = options.Timeout.HasValue && !ct.IsCancellationRequested;
            var parseProblem = isTimeout && options.Timeout != null
                ? new Problem("Timeout", $"Request timed out after {options.Timeout.Value.TotalMilliseconds:F0} ms.", null, null)
                : new Problem("DeserializeError", "Deserialization was canceled or timed out.");
            envelope = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, parseProblem);
        }
        catch (JsonException je)
        {
            var parseProblem = new Problem("DeserializeError", $"Failed to parse response: {je.Message}");
            envelope = new Envelope<TResponse>(operation, correlationId, DateTimeOffset.UtcNow, default, parseProblem);
        }
        catch (Exception ex)
        {
            var parseProblem = new Problem("DeserializeError", $"Unexpected error during deserialization: {ex.Message}");
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

public class McpTransportException(Problem problem, HttpStatusCode? statusCode = null, Exception? inner = null) : Exception(problem.Message, inner)
{
    public Problem Problem { get; } = problem;
    public HttpStatusCode? StatusCode { get; } = statusCode;
}

public class McpApplicationException(Problem problem, HttpStatusCode? statusCode = null, Exception? inner = null) : Exception(problem.Message, inner)
{
    public Problem Problem { get; } = problem;
    public HttpStatusCode? StatusCode { get; } = statusCode;
}
