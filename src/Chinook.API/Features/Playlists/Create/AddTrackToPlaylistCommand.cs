using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Playlists;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record AddTrackToPlaylistRequest(int TrackId);

public sealed record AddTrackToPlaylistCommand(int PlaylistId, int TrackId) : IResultCommand<PlaylistDto>;

public sealed record AddTrackToPlaylistCommandHandler(
    ChinookDbContext dbContext,
    ILogger<AddTrackToPlaylistCommandHandler> logger) : IResultCommandHandler<AddTrackToPlaylistCommand, PlaylistDto>
{
    public async Task<Result<PlaylistDto>> Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[AddTrackToPlaylistCommandHandler.Handle] - Handling AddTrackToPlaylistCommand for PlaylistId: {PlaylistId}, TrackId: {TrackId}", request.PlaylistId, request.TrackId);

        var playlist = await dbContext.Playlists
            .Where(p => p.PlaylistId == request.PlaylistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            logger.LogWarning("[AddTrackToPlaylistCommandHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var trackExists = await dbContext.Tracks
            .AsNoTracking()
            .AnyAsync(t => t.TrackId == request.TrackId, cancellationToken);

        if (!trackExists)
        {
            logger.LogWarning("[AddTrackToPlaylistCommandHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        var alreadyAdded = await dbContext.PlaylistTracks
            .AsNoTracking()
            .AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken);

        if (alreadyAdded)
        {
            logger.LogWarning("[AddTrackToPlaylistCommandHandler.Handle] - Track {TrackId} is already in Playlist {PlaylistId}", request.TrackId, request.PlaylistId);
            return Result.Fail($"Track with ID {request.TrackId} is already in playlist {request.PlaylistId}.");
        }

        dbContext.PlaylistTracks.Add(new PlaylistTrack { PlaylistId = request.PlaylistId, TrackId = request.TrackId });
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[AddTrackToPlaylistCommandHandler.Handle] - Successfully added TrackId: {TrackId} to PlaylistId: {PlaylistId}", request.TrackId, request.PlaylistId);
        return Result.Ok(new PlaylistDto(playlist.PlaylistId, playlist.Name));
    }
}
