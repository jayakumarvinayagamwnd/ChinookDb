using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListArtistsQuery : IResultQuery<List<ArtistDto>>, ICacheableQuery
{
    public string CacheKey => "catalog:artists:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}
public sealed record ListArtistsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListArtistsQueryHandler> logger) : IResultQueryHandler<ListArtistsQuery, List<ArtistDto>>
{
    public async Task<Result<List<ArtistDto>>> Handle(ListArtistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListArtistsQueryHandler.Handle] - Handling ListArtistsQuery");
        var artists = await dbContext.Artists
            .AsNoTracking()
            .ProjectTo<ArtistDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        logger.LogInformation("[ListArtistsQueryHandler.Handle] - Successfully retrieved {ArtistCount} artists", artists.Count);
        return Result.Ok(artists);
    }
}