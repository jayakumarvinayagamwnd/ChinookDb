namespace Chinook.API.Common.Pagination;

public sealed record OffsetPagedResponse<T>(IReadOnlyList<T> Items, int Offset, int Limit, int TotalCount)
{
    public bool HasMore => Offset + Items.Count < TotalCount;
}