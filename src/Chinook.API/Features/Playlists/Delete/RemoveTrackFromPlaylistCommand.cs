using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record RemoveTrackFromPlaylistCommand(int PlaylistId, int TrackId) : IResultCommand<bool>;

public sealed record RemoveTrackFromPlaylistCommandHandler(
    ChinookDbContext dbContext,
    ILogger<RemoveTrackFromPlaylistCommandHandler> logger) : IResultCommandHandler<RemoveTrackFromPlaylistCommand, bool>
{
    public async Task<Result<bool>> Handle(RemoveTrackFromPlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[RemoveTrackFromPlaylistCommandHandler.Handle] - Handling RemoveTrackFromPlaylistCommand for PlaylistId: {PlaylistId}, TrackId: {TrackId}", request.PlaylistId, request.TrackId);

        var playlistTrack = await dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId)
            .SingleOrDefaultAsync(cancellationToken);

        if (playlistTrack is null)
        {
            logger.LogWarning("[RemoveTrackFromPlaylistCommandHandler.Handle] - Track {TrackId} not found in Playlist {PlaylistId}", request.TrackId, request.PlaylistId);
            return Result.Fail($"Track with ID {request.TrackId} was not found in playlist {request.PlaylistId}.");
        }

        dbContext.PlaylistTracks.Remove(playlistTrack);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[RemoveTrackFromPlaylistCommandHandler.Handle] - Successfully removed TrackId: {TrackId} from PlaylistId: {PlaylistId}", request.TrackId, request.PlaylistId);
        return Result.Ok(true);
    }
}
