using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record DeletePlaylistCommand(int PlaylistId) : IResultCommand<bool>;

public sealed record DeletePlaylistCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeletePlaylistCommandHandler> logger) : IResultCommandHandler<DeletePlaylistCommand, bool>
{
    public async Task<Result<bool>> Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeletePlaylistCommandHandler.Handle] - Handling DeletePlaylistCommand for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlist = await dbContext.Playlists
            .Where(p => p.PlaylistId == request.PlaylistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            logger.LogWarning("[DeletePlaylistCommandHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var playlistTracks = await dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .ToListAsync(cancellationToken);

        dbContext.PlaylistTracks.RemoveRange(playlistTracks);
        dbContext.Playlists.Remove(playlist);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[DeletePlaylistCommandHandler.Handle] - Delete failed due to related records for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} cannot be deleted because related records exist.");
        }

        logger.LogInformation("[DeletePlaylistCommandHandler.Handle] - Successfully deleted playlist with PlaylistId: {PlaylistId}", request.PlaylistId);
        return Result.Ok(true);
    }
}
