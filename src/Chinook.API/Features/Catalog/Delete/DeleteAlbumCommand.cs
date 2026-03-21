using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record DeleteAlbumCommand(int AlbumId) : IResultCommand<bool>;

public sealed record DeleteAlbumCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeleteAlbumCommandHandler> logger) : IResultCommandHandler<DeleteAlbumCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeleteAlbumCommandHandler.Handle] - Handling DeleteAlbumCommand for AlbumId: {AlbumId}", request.AlbumId);

        var album = await dbContext.Albums
            .Where(a => a.AlbumId == request.AlbumId)
            .SingleOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            logger.LogWarning("[DeleteAlbumCommandHandler.Handle] - Album not found for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} not found.");
        }

        var hasTracks = await dbContext.Tracks
            .AsNoTracking()
            .AnyAsync(t => t.AlbumId == request.AlbumId, cancellationToken);

        if (hasTracks)
        {
            logger.LogWarning("[DeleteAlbumCommandHandler.Handle] - Cannot delete AlbumId: {AlbumId} because dependent tracks exist", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} cannot be deleted because it has related tracks.");
        }

        dbContext.Albums.Remove(album);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[DeleteAlbumCommandHandler.Handle] - Delete failed due to related records for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} cannot be deleted because related records exist.");
        }

        logger.LogInformation("[DeleteAlbumCommandHandler.Handle] - Successfully deleted album with AlbumId: {AlbumId}", request.AlbumId);
        return Result.Ok(true);
    }
}
