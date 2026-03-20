using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;
public sealed record ListArtistsQuery : IQuery<List<ArtistDto>>;
public sealed record ListArtistsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListArtistsQueryHandler> logger) : IQueryHandler<ListArtistsQuery, List<ArtistDto>>
{
    public async Task<List<ArtistDto>> Handle(ListArtistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListArtistsQueryHandler.Handle] - Handling ListArtistsQuery");
        return await dbContext.Artists
            .AsNoTracking()
            .ProjectTo<ArtistDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}