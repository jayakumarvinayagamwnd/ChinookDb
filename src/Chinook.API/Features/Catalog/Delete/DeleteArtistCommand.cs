using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record DeleteArtistCommand(int ArtistId) : IResultCommand<bool>;

public sealed record DeleteArtistCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeleteArtistCommandHandler> logger) : IResultCommandHandler<DeleteArtistCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteArtistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeleteArtistCommandHandler.Handle] - Handling DeleteArtistCommand for ArtistId: {ArtistId}", request.ArtistId);

        var artist = await dbContext.Artists
            .Where(a => a.ArtistId == request.ArtistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (artist is null)
        {
            logger.LogWarning("[DeleteArtistCommandHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        var hasAlbums = await dbContext.Albums
            .AsNoTracking()
            .AnyAsync(a => a.ArtistId == request.ArtistId, cancellationToken);

        if (hasAlbums)
        {
            logger.LogWarning("[DeleteArtistCommandHandler.Handle] - Cannot delete ArtistId: {ArtistId} because dependent albums exist", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} cannot be deleted because it has related albums.");
        }

        dbContext.Artists.Remove(artist);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[DeleteArtistCommandHandler.Handle] - Delete failed due to related records for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} cannot be deleted because related records exist.");
        }

        logger.LogInformation("[DeleteArtistCommandHandler.Handle] - Successfully deleted artist with ArtistId: {ArtistId}", request.ArtistId);
        return Result.Ok(true);
    }
}
