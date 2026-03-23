namespace Chinook.API.Common.Pagination;

public interface IOffsetPaginatedQuery
{
    int Offset { get; }
    int Limit { get; }
}