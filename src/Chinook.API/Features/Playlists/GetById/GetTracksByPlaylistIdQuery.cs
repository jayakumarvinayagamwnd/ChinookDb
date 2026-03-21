using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Features.Catalog;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record GetTracksByPlaylistIdQuery(int PlaylistId) : IResultQuery<List<TrackDto>>, ICacheableQuery
{
    public string CacheKey => $"playlists:{PlaylistId}:tracks";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetTracksByPlaylistIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetTracksByPlaylistIdQueryHandler> logger) : IResultQueryHandler<GetTracksByPlaylistIdQuery, List<TrackDto>>
{
    public async Task<Result<List<TrackDto>>> Handle(GetTracksByPlaylistIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTracksByPlaylistIdQueryHandler.Handle] - Handling GetTracksByPlaylistIdQuery for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlistExists = await dbContext.Playlists
            .AsNoTracking()
            .AnyAsync(p => p.PlaylistId == request.PlaylistId, cancellationToken);

        if (!playlistExists)
        {
            logger.LogWarning("[GetTracksByPlaylistIdQueryHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var trackIds = dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .Select(pt => pt.TrackId);

        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => trackIds.Contains(t.TrackId))
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetTracksByPlaylistIdQueryHandler.Handle] - Retrieved {TrackCount} tracks for PlaylistId: {PlaylistId}", tracks.Count, request.PlaylistId);
        return Result.Ok(tracks);
    }
}
