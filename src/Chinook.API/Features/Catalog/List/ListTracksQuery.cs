using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListTracksQuery : IResultQuery<List<TrackDto>>, ICacheableQuery
{
    public string CacheKey => "catalog:tracks:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListTracksQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListTracksQueryHandler> logger) : IResultQueryHandler<ListTracksQuery, List<TrackDto>>
{
    public async Task<Result<List<TrackDto>>> Handle(ListTracksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListTracksQueryHandler.Handle] - Handling ListTracksQuery");

        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListTracksQueryHandler.Handle] - Successfully retrieved {TrackCount} tracks", tracks.Count);
        return Result.Ok(tracks);
    }
}
