using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListAlbumsQuery : IResultQuery<List<AlbumDto>>, ICacheableQuery
{
    public string CacheKey => "catalog:albums:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListAlbumsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListAlbumsQueryHandler> logger) : IResultQueryHandler<ListAlbumsQuery, List<AlbumDto>>
{
    public async Task<Result<List<AlbumDto>>> Handle(ListAlbumsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListAlbumsQueryHandler.Handle] - Handling ListAlbumsQuery");

        var albums = await dbContext.Albums
            .AsNoTracking()
            .ProjectTo<AlbumDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListAlbumsQueryHandler.Handle] - Successfully retrieved {AlbumCount} albums", albums.Count);
        return Result.Ok(albums);
    }
}
