using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListGenresQuery : IResultQuery<List<GenreDto>>, ICacheableQuery
{
    public string CacheKey => "catalog:genres:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public sealed record ListGenresQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListGenresQueryHandler> logger) : IResultQueryHandler<ListGenresQuery, List<GenreDto>>
{
    public async Task<Result<List<GenreDto>>> Handle(ListGenresQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListGenresQueryHandler.Handle] - Handling ListGenresQuery");

        var genres = await dbContext.Genres
            .AsNoTracking()
            .ProjectTo<GenreDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListGenresQueryHandler.Handle] - Successfully retrieved {GenreCount} genres", genres.Count);
        return Result.Ok(genres);
    }
}
