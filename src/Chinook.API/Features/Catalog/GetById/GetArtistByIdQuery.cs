using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetArtistByIdQuery(int ArtistId) : IQuery<ArtistDto?>;

public sealed record GetArtistByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetArtistByIdQueryHandler> logger) : IQueryHandler<GetArtistByIdQuery, ArtistDto?>
{
    public async Task<ArtistDto?> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetArtistByIdQueryHandler.Handle] - Handling GetArtistByIdQuery for ArtistId: {ArtistId}", request.ArtistId);

        return await dbContext.Artists
            .AsNoTracking()
            .Where(a => a.ArtistId == request.ArtistId)
            .ProjectTo<ArtistDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
