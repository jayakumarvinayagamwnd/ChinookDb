using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetTopTracksQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<TopTrackDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"analytics:top-tracks:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetTopTracksQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetTopTracksQueryHandler> logger) : IResultQueryHandler<GetTopTracksQuery, OffsetPagedResponse<TopTrackDto>>
{
    public async Task<Result<OffsetPagedResponse<TopTrackDto>>> Handle(GetTopTracksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTopTracksQueryHandler.Handle] - Handling GetTopTracksQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var topTracks = await dbContext.InvoiceLines
            .AsNoTracking()
            .Join(dbContext.Tracks.AsNoTracking(), line => line.TrackId, track => track.TrackId, (line, track) => new
            {
                track.TrackId,
                TrackName = track.Name,
                line.Quantity,
                Revenue = line.UnitPrice * line.Quantity
            })
            .GroupBy(x => new { x.TrackId, x.TrackName })
            .Select(g => new TopTrackDto(
                g.Key.TrackId,
                g.Key.TrackName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Revenue)))
            .OrderByDescending(x => x.UnitsSold)
            .ThenByDescending(x => x.Revenue)
            .ThenBy(x => x.TrackId)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[GetTopTracksQueryHandler.Handle] - Successfully retrieved {TrackCount} tracks out of {TotalCount}", topTracks.Items.Count, topTracks.TotalCount);
        return Result.Ok(topTracks);
    }
}
