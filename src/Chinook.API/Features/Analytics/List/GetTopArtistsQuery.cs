using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetTopArtistsQuery(int Limit = 10) : IResultQuery<List<TopArtistDto>>, ICacheableQuery
{
    public string CacheKey => $"analytics:top-artists:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetTopArtistsQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetTopArtistsQueryHandler> logger) : IResultQueryHandler<GetTopArtistsQuery, List<TopArtistDto>>
{
    public async Task<Result<List<TopArtistDto>>> Handle(GetTopArtistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTopArtistsQueryHandler.Handle] - Handling GetTopArtistsQuery with Limit: {Limit}", request.Limit);

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
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetTopArtistsQueryHandler.Handle] - Successfully retrieved {ArtistCount} artists", topArtists.Count);
        return Result.Ok(topArtists);
    }
}
