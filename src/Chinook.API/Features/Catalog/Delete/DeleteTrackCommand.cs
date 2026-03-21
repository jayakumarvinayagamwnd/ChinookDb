using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record DeleteTrackCommand(int TrackId) : IResultCommand<bool>;

public sealed record DeleteTrackCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeleteTrackCommandHandler> logger) : IResultCommandHandler<DeleteTrackCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeleteTrackCommandHandler.Handle] - Handling DeleteTrackCommand for TrackId: {TrackId}", request.TrackId);

        var track = await dbContext.Tracks
            .Where(t => t.TrackId == request.TrackId)
            .SingleOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            logger.LogWarning("[DeleteTrackCommandHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        var usedInInvoices = await dbContext.InvoiceLines
            .AsNoTracking()
            .AnyAsync(i => i.TrackId == request.TrackId, cancellationToken);

        if (usedInInvoices)
        {
            logger.LogWarning("[DeleteTrackCommandHandler.Handle] - Cannot delete TrackId: {TrackId} because invoice items exist", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} cannot be deleted because it is referenced by invoice items.");
        }

        var usedInPlaylists = await dbContext.PlaylistTracks
            .AsNoTracking()
            .AnyAsync(p => p.TrackId == request.TrackId, cancellationToken);

        if (usedInPlaylists)
        {
            logger.LogWarning("[DeleteTrackCommandHandler.Handle] - Cannot delete TrackId: {TrackId} because playlist entries exist", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} cannot be deleted because it is referenced by playlists.");
        }

        dbContext.Tracks.Remove(track);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[DeleteTrackCommandHandler.Handle] - Delete failed due to related records for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} cannot be deleted because related records exist.");
        }

        logger.LogInformation("[DeleteTrackCommandHandler.Handle] - Successfully deleted track with TrackId: {TrackId}", request.TrackId);
        return Result.Ok(true);
    }
}
