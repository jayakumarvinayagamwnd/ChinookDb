using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record PublishAlbumCommand(int AlbumId) : IResultCommand<AlbumDto>;

public sealed record PublishAlbumCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<PublishAlbumCommandHandler> logger) : IResultCommandHandler<PublishAlbumCommand, AlbumDto>
{
    public async Task<Result<AlbumDto>> Handle(PublishAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[PublishAlbumCommandHandler.Handle] - Handling PublishAlbumCommand for AlbumId: {AlbumId}", request.AlbumId);

        var album = await dbContext.Albums
            .AsNoTracking()
            .Where(a => a.AlbumId == request.AlbumId)
            .SingleOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            logger.LogWarning("[PublishAlbumCommandHandler.Handle] - Album not found for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} not found.");
        }

        var hasTracks = await dbContext.Tracks
            .AsNoTracking()
            .AnyAsync(t => t.AlbumId == request.AlbumId, cancellationToken);

        if (!hasTracks)
        {
            logger.LogWarning("[PublishAlbumCommandHandler.Handle] - AlbumId: {AlbumId} cannot be published because it has no tracks", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} cannot be published because it has no tracks.");
        }

        logger.LogInformation("[PublishAlbumCommandHandler.Handle] - Successfully published album with AlbumId: {AlbumId}", request.AlbumId);
        return Result.Ok(mapper.Map<AlbumDto>(album));
    }
}
