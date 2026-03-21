using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetArtistByIdQuery(int ArtistId) : IResultQuery<ArtistDto>, ICacheableQuery
{
    public string CacheKey => $"catalog:artist:{ArtistId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetArtistByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetArtistByIdQueryHandler> logger) : IResultQueryHandler<GetArtistByIdQuery, ArtistDto>
{
    public async Task<Result<ArtistDto>> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetArtistByIdQueryHandler.Handle] - Handling GetArtistByIdQuery for ArtistId: {ArtistId}", request.ArtistId);

        var artist = await dbContext.Artists
            .AsNoTracking()
            .Where(a => a.ArtistId == request.ArtistId)
            .ProjectTo<ArtistDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
            
        if (artist is null)
        {
            logger.LogWarning("[GetArtistByIdQueryHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        logger.LogInformation("[GetArtistByIdQueryHandler.Handle] - Successfully retrieved artist for ArtistId: {ArtistId}", request.ArtistId);
        return Result.Ok(artist);
    }
}
