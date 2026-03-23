using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetTopArtistsQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<TopArtistDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"analytics:top-artists:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetTopArtistsQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetTopArtistsQueryHandler> logger) : IResultQueryHandler<GetTopArtistsQuery, OffsetPagedResponse<TopArtistDto>>
{
    public async Task<Result<OffsetPagedResponse<TopArtistDto>>> Handle(GetTopArtistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTopArtistsQueryHandler.Handle] - Handling GetTopArtistsQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var topArtists = await dbContext.InvoiceLines
            .AsNoTracking()
            .Join(dbContext.Tracks.AsNoTracking(), line => line.TrackId, track => track.TrackId, (line, track) => new
            {
                line.Quantity,
                line.UnitPrice,
                track.AlbumId
            })
            .Where(lt => lt.AlbumId.HasValue)
            .Join(dbContext.Albums.AsNoTracking(), lt => lt.AlbumId!.Value, album => album.AlbumId, (lt, album) => new
            {
                lt.Quantity,
                lt.UnitPrice,
                album.ArtistId
            })
            .Join(dbContext.Artists.AsNoTracking(), lta => lta.ArtistId, artist => artist.ArtistId, (lta, artist) => new
            {
                artist.ArtistId,
                ArtistName = artist.Name,
                lta.Quantity,
                Revenue = lta.UnitPrice * lta.Quantity
            })
            .GroupBy(x => new { x.ArtistId, x.ArtistName })
            .Select(g => new TopArtistDto(
                g.Key.ArtistId,
                g.Key.ArtistName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Revenue)))
            .OrderByDescending(x => x.UnitsSold)
            .ThenByDescending(x => x.Revenue)
            .ThenBy(x => x.ArtistId)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[GetTopArtistsQueryHandler.Handle] - Successfully retrieved {ArtistCount} artists out of {TotalCount}", topArtists.Items.Count, topArtists.TotalCount);
        return Result.Ok(topArtists);
    }
}
