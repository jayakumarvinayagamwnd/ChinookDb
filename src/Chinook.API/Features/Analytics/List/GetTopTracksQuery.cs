using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetTopTracksQuery(int Limit = 10) : IResultQuery<List<TopTrackDto>>, ICacheableQuery
{
    public string CacheKey => $"analytics:top-tracks:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetTopTracksQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetTopTracksQueryHandler> logger) : IResultQueryHandler<GetTopTracksQuery, List<TopTrackDto>>
{
    public async Task<Result<List<TopTrackDto>>> Handle(GetTopTracksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTopTracksQueryHandler.Handle] - Handling GetTopTracksQuery with Limit: {Limit}", request.Limit);

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
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetTopTracksQueryHandler.Handle] - Successfully retrieved {TrackCount} tracks", topTracks.Count);
        return Result.Ok(topTracks);
    }
}
