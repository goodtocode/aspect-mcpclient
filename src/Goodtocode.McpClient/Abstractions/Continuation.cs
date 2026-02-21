namespace Goodtocode.McpClient.Abstractions;

public class Continuation(string token, int? nextPageSize = null)
{
    public string Token { get; set; } = token;
    public int? NextPageSize { get; set; } = nextPageSize;
}
