using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListAlbumsQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<AlbumDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"catalog:albums:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListAlbumsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListAlbumsQueryHandler> logger) : IResultQueryHandler<ListAlbumsQuery, OffsetPagedResponse<AlbumDto>>
{
    public async Task<Result<OffsetPagedResponse<AlbumDto>>> Handle(ListAlbumsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListAlbumsQueryHandler.Handle] - Handling ListAlbumsQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var albums = await dbContext.Albums
            .AsNoTracking()
            .OrderBy(album => album.Title)
            .ThenBy(album => album.AlbumId)
            .ProjectTo<AlbumDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListAlbumsQueryHandler.Handle] - Successfully retrieved {AlbumCount} albums out of {TotalCount}", albums.Items.Count, albums.TotalCount);
        return Result.Ok(albums);
    }
}
