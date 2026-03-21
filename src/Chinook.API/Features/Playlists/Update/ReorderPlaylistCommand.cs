using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Features.Catalog;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record ReorderPlaylistRequest(List<int> TrackIds);

public sealed record ReorderPlaylistCommand(int PlaylistId, List<int> TrackIds) : IResultCommand<List<TrackDto>>;

public sealed record ReorderPlaylistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ReorderPlaylistCommandHandler> logger) : IResultCommandHandler<ReorderPlaylistCommand, List<TrackDto>>
{
    public async Task<Result<List<TrackDto>>> Handle(ReorderPlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ReorderPlaylistCommandHandler.Handle] - Handling ReorderPlaylistCommand for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlistExists = await dbContext.Playlists
            .AsNoTracking()
            .AnyAsync(p => p.PlaylistId == request.PlaylistId, cancellationToken);

        if (!playlistExists)
        {
            logger.LogWarning("[ReorderPlaylistCommandHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var currentTrackIds = await dbContext.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .Select(pt => pt.TrackId)
            .ToListAsync(cancellationToken);

        var requestedSet = request.TrackIds.ToHashSet();
        var currentSet = currentTrackIds.ToHashSet();

        if (!requestedSet.SetEquals(currentSet))
        {
            logger.LogWarning("[ReorderPlaylistCommandHandler.Handle] - Reorder TrackIds do not match playlist tracks for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"The provided TrackIds must exactly match the tracks currently in playlist {request.PlaylistId}.");
        }

        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => requestedSet.Contains(t.TrackId))
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var ordered = request.TrackIds
            .Select(id => tracks.First(t => t.TrackId == id))
            .ToList();

        logger.LogInformation("[ReorderPlaylistCommandHandler.Handle] - Successfully reordered {TrackCount} tracks for PlaylistId: {PlaylistId}", ordered.Count, request.PlaylistId);
        return Result.Ok(ordered);
    }
}
