using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Common.Pagination;

public static class QueryablePaginationExtensions
{
    public static async Task<OffsetPagedResponse<T>> ToOffsetPagedResponseAsync<T>(
        this IQueryable<T> query,
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new OffsetPagedResponse<T>(items, offset, limit, totalCount);
    }
}