using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record UpdateArtistRequest(string Name);

public sealed record UpdateArtistCommand(int ArtistId, string Name) : IResultCommand<ArtistDto>;

public sealed record UpdateArtistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateArtistCommandHandler> logger) : IResultCommandHandler<UpdateArtistCommand, ArtistDto>
{
    public async Task<Result<ArtistDto>> Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateArtistCommandHandler.Handle] - Handling UpdateArtistCommand for ArtistId: {ArtistId}", request.ArtistId);

        var artist = await dbContext.Artists
            .Where(a => a.ArtistId == request.ArtistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (artist is null)
        {
            logger.LogWarning("[UpdateArtistCommandHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        mapper.Map(request, artist);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateArtistCommandHandler.Handle] - Successfully updated artist with ArtistId: {ArtistId}", artist.ArtistId);

        return Result.Ok(mapper.Map<ArtistDto>(artist));
    }
}
