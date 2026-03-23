using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListArtistsQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<ArtistDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"catalog:artists:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}
public sealed record ListArtistsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListArtistsQueryHandler> logger) : IResultQueryHandler<ListArtistsQuery, OffsetPagedResponse<ArtistDto>>
{
    public async Task<Result<OffsetPagedResponse<ArtistDto>>> Handle(ListArtistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListArtistsQueryHandler.Handle] - Handling ListArtistsQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);
        var artists = await dbContext.Artists
            .AsNoTracking()
            .OrderBy(artist => artist.Name)
            .ThenBy(artist => artist.ArtistId)
            .ProjectTo<ArtistDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);
        
        logger.LogInformation("[ListArtistsQueryHandler.Handle] - Successfully retrieved {ArtistCount} artists out of {TotalCount}", artists.Items.Count, artists.TotalCount);
        return Result.Ok(artists);
    }
}