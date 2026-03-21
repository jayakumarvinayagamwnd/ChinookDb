using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Features.Catalog;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record GetPlaylistRecommendationsQuery(int PlaylistId) : IResultQuery<List<TrackDto>>, ICacheableQuery
{
    public string CacheKey => $"playlists:{PlaylistId}:recommendations";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetPlaylistRecommendationsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetPlaylistRecommendationsQueryHandler> logger) : IResultQueryHandler<GetPlaylistRecommendationsQuery, List<TrackDto>>
{
    private const int RecommendationLimit = 10;

    public async Task<Result<List<TrackDto>>> Handle(GetPlaylistRecommendationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetPlaylistRecommendationsQueryHandler.Handle] - Handling GetPlaylistRecommendationsQuery for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlistExists = await dbContext.Playlists
            .AsNoTracking()
            .AnyAsync(p => p.PlaylistId == request.PlaylistId, cancellationToken);

        if (!playlistExists)
        {
            logger.LogWarning("[GetPlaylistRecommendationsQueryHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var playlistTrackIdQuery = dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .Select(pt => pt.TrackId);

        var genreIds = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => t.GenreId.HasValue && playlistTrackIdQuery.Contains(t.TrackId))
            .Select(t => t.GenreId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var recommendations = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => t.GenreId.HasValue
                        && genreIds.Contains(t.GenreId.Value)
                        && !playlistTrackIdQuery.Contains(t.TrackId))
            .OrderBy(t => t.Name)
            .Take(RecommendationLimit)
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetPlaylistRecommendationsQueryHandler.Handle] - Retrieved {Count} recommendations for PlaylistId: {PlaylistId}", recommendations.Count, request.PlaylistId);
        return Result.Ok(recommendations);
    }
}
