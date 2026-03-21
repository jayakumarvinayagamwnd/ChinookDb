using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record CreateAlbumCommand(string Title, int ArtistId) : IResultCommand<AlbumDto>;

public sealed record CreateAlbumCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateAlbumCommandHandler> logger) : IResultCommandHandler<CreateAlbumCommand, AlbumDto>
{
    public async Task<Result<AlbumDto>> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateAlbumCommandHandler.Handle] - Handling CreateAlbumCommand for Title: {Title}, ArtistId: {ArtistId}", request.Title, request.ArtistId);

        var artistExists = await dbContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.ArtistId == request.ArtistId, cancellationToken);

        if (!artistExists)
        {
            logger.LogWarning("[CreateAlbumCommandHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        var album = mapper.Map<Album>(request);
        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateAlbumCommandHandler.Handle] - Successfully created album with AlbumId: {AlbumId}", album.AlbumId);
        return Result.Ok(mapper.Map<AlbumDto>(album));
    }
}
