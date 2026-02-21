using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Goodtocode.McpClient.Abstractions;

public interface IMcpSerializer
{
    string ContentType { get; }
    string Serialize<T>(T value);
    T? Deserialize<T>(string json);
    HttpContent ToHttpContent<T>(T value);
    Task<T?> ReadFromHttpAsync<T>(HttpContent content, CancellationToken ct);
}

public class SystemTextJsonMcpSerializer : IMcpSerializer
{
    public string ContentType => "application/json";

    public JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);

    public HttpContent ToHttpContent<T>(T value)
        => new StringContent(Serialize(value), Encoding.UTF8, ContentType);

    public async Task<T?> ReadFromHttpAsync<T>(HttpContent content, CancellationToken ct)
    {
        var str = await content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(str)) return default;
        return Deserialize<T>(str);
    }
}
