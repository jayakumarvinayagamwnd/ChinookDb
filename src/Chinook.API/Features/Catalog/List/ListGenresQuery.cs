using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListGenresQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<GenreDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"catalog:genres:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public sealed record ListGenresQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListGenresQueryHandler> logger) : IResultQueryHandler<ListGenresQuery, OffsetPagedResponse<GenreDto>>
{
    public async Task<Result<OffsetPagedResponse<GenreDto>>> Handle(ListGenresQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListGenresQueryHandler.Handle] - Handling ListGenresQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var genres = await dbContext.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .ThenBy(genre => genre.GenreId)
            .ProjectTo<GenreDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListGenresQueryHandler.Handle] - Successfully retrieved {GenreCount} genres out of {TotalCount}", genres.Items.Count, genres.TotalCount);
        return Result.Ok(genres);
    }
}
