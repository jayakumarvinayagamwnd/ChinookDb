using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;
using FluentResults;

namespace Chinook.API.Features.Catalog;

public sealed record CreateArtistCommand(string Name) : IResultCommand<ArtistDto>;

public sealed record CreateArtistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateArtistCommandHandler> logger) : IResultCommandHandler<CreateArtistCommand, ArtistDto>
{
    public async Task<Result<ArtistDto>> Handle(CreateArtistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateArtistCommandHandler.Handle] - Handling CreateArtistCommand for Name: {Name}", request.Name);

        var artist = mapper.Map<Artist>(request);
        dbContext.Artists.Add(artist);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateArtistCommandHandler.Handle] - Successfully created artist with ArtistId: {ArtistId}", artist.ArtistId);

        return Result.Ok(mapper.Map<ArtistDto>(artist));
    }
}
