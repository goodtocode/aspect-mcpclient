# Goodtocode.McpClient

[![NuGet CI/CD](https://github.com/goodtocode/aspect-mcpclient/actions/workflows/gtc-mcpclient-nuget.yml/badge.svg)](https://github.com/goodtocode/aspect-mcpclient/actions/workflows/gtc-mcpclient-nuget.yml)

A standardized, resilient client library for Model Context Protocol (MCP) communication in .NET. Designed for developers who need reliable, typed, and transport-agnostic messaging between MCP-compliant AI agents and services.

## Features
- Strongly-typed envelope model for all MCP operations
- Transport-agnostic: works with HTTP, gRPC, and other protocols
- Built-in support for result and problem handling
- Continuation and paging for large data sets
- Batch operations for efficient multi-message processing
- Extensible serialization via `IMcpSerializer`
- Easy integration with .NET DI and HttpClient
- Compatible with custom delegating handlers (e.g., for authentication)

## Installation

Install via NuGet:

    dotnet add package Goodtocode.McpClient

## Usage

### 1. Register McpClient with `IServiceCollection`

You can register your own `HttpClient` and custom delegating handler (such as from `Goodtocode.SecuredHttpClient`) for secure, resilient communication:

    using Goodtocode.McpClient.Client;
    using Goodtocode.SecuredHttpClient; // For token handler
    using Microsoft.Extensions.DependencyInjection;

    // Register a secured HttpClient for MCP communication
    services.AddHttpClient("McpSecuredClient", client =>
    {
        client.BaseAddress = new Uri("https://mcp-agent.example.com");
    })
    .AddHttpMessageHandler<TokenHandler>(); // Provided by Goodtocode.SecuredHttpClient

    // Register McpHttpClient using the named HttpClient
    services.AddTransient<IMcpClient>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("McpSecuredClient");
        return new McpHttpClient(httpClient);
    });

### 2. Send a Typed MCP Request

    using Goodtocode.McpClient.Client;

    var mcpClient = serviceProvider.GetRequiredService<IMcpClient>();

    var request = new MyRequestType { /* ... */ };
    var envelope = await mcpClient.SendAsync<MyRequestType, MyResponseType>(
        operation: "my-operation",
        path: "/api/endpoint",
        request: request
    );

    if (envelope.HasResult)
    {
        var result = envelope.Result;
        // Handle result
    }
    else if (envelope.HasProblem)
    {
        var problem = envelope.Problem;
        // Handle error
    }

### 3. Envelope Model

All MCP responses are wrapped in an `Envelope<T>`:

    public class Envelope<T>
    {
        public string Operation { get; }
        public string CorrelationId { get; }
        public DateTimeOffset SentUtc { get; }
        public T? Result { get; }
        public Problem? Problem { get; }
        public Continuation? Continue { get; }
        public IReadOnlyDictionary<string, string>? Metadata { get; }
        public bool HasResult => Result != null && Problem == null;
        public bool HasProblem => Problem != null;
    }

### 4. Paging and Batch Support

- Use `PageResult<T>` for paged data.
- Use `BatchItem<T>` for batch operations.

### 5. Custom Serialization

You can provide your own serializer by implementing `IMcpSerializer` and passing it to `McpHttpClient`.

### 6. Communicating Between MCP Agents

- Standardize all requests and responses using the envelope model.
- Use correlation IDs for traceability.
- Handle problems and errors using the `Problem` type.
- Use continuation tokens for streaming or paged data.
- Secure communication with delegating handlers (e.g., `TokenHandler` from `Goodtocode.SecuredHttpClient`).

## Options

- `McpSendOptions`: Control correlation, idempotency, timeout, and headers.
- `IMcpSerializer`: Plug in custom serialization (default: System.Text.Json).

## License

MIT

## Contact

- [GitHub Repo](https://github.com/goodtocode/aspect-mcpclient)
- [@goodtocode](https://twitter.com/goodtocode)

## Version History

| Version | Date       | Changes                       |
|---------|------------|-------------------------------|
| 1.1.0   | 2026-01-22 | Bump from .NET 9 to .NET 10   |