namespace Goodtocode.McpClient.Messaging;

public class PageResult<TItem>(IReadOnlyList<TItem> items, int pageNumber, int pageSize, long? totalCount = null)
{
    public IReadOnlyList<TItem> Items { get; set; } = items;
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public long? TotalCount { get; set; } = totalCount;
}
