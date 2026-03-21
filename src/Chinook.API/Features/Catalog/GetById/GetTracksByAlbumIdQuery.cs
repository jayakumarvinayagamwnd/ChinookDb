using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetTracksByAlbumIdQuery(int AlbumId) : IResultQuery<List<TrackDto>>, ICacheableQuery
{
    public string CacheKey => $"catalog:album:{AlbumId}:tracks";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetTracksByAlbumIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetTracksByAlbumIdQueryHandler> logger) : IResultQueryHandler<GetTracksByAlbumIdQuery, List<TrackDto>>
{
    public async Task<Result<List<TrackDto>>> Handle(GetTracksByAlbumIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTracksByAlbumIdQueryHandler.Handle] - Handling GetTracksByAlbumIdQuery for AlbumId: {AlbumId}", request.AlbumId);

        var albumExists = await dbContext.Albums
            .AsNoTracking()
            .AnyAsync(a => a.AlbumId == request.AlbumId, cancellationToken);

        if (!albumExists)
        {
            logger.LogWarning("[GetTracksByAlbumIdQueryHandler.Handle] - Album not found for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} not found.");
        }

        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => t.AlbumId == request.AlbumId)
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetTracksByAlbumIdQueryHandler.Handle] - Retrieved {TrackCount} tracks for AlbumId: {AlbumId}", tracks.Count, request.AlbumId);
        return Result.Ok(tracks);
    }
}
