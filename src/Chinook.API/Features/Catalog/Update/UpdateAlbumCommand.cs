using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record UpdateAlbumRequest(string Title, int ArtistId);

public sealed record UpdateAlbumCommand(int AlbumId, string Title, int ArtistId) : IResultCommand<AlbumDto>;

public sealed record UpdateAlbumCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateAlbumCommandHandler> logger) : IResultCommandHandler<UpdateAlbumCommand, AlbumDto>
{
    public async Task<Result<AlbumDto>> Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateAlbumCommandHandler.Handle] - Handling UpdateAlbumCommand for AlbumId: {AlbumId}", request.AlbumId);

        var album = await dbContext.Albums
            .Where(a => a.AlbumId == request.AlbumId)
            .SingleOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            logger.LogWarning("[UpdateAlbumCommandHandler.Handle] - Album not found for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} not found.");
        }

        var artistExists = await dbContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.ArtistId == request.ArtistId, cancellationToken);

        if (!artistExists)
        {
            logger.LogWarning("[UpdateAlbumCommandHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        mapper.Map(request, album);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateAlbumCommandHandler.Handle] - Successfully updated album with AlbumId: {AlbumId}", album.AlbumId);
        return Result.Ok(mapper.Map<AlbumDto>(album));
    }
}
