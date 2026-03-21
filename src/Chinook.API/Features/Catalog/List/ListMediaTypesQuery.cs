using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListMediaTypesQuery : IResultQuery<List<MediaTypeDto>>, ICacheableQuery
{
    public string CacheKey => "catalog:media-types:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public sealed record ListMediaTypesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListMediaTypesQueryHandler> logger) : IResultQueryHandler<ListMediaTypesQuery, List<MediaTypeDto>>
{
    public async Task<Result<List<MediaTypeDto>>> Handle(ListMediaTypesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListMediaTypesQueryHandler.Handle] - Handling ListMediaTypesQuery");

        var mediaTypes = await dbContext.MediaTypes
            .AsNoTracking()
            .ProjectTo<MediaTypeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListMediaTypesQueryHandler.Handle] - Successfully retrieved {MediaTypeCount} media types", mediaTypes.Count);
        return Result.Ok(mediaTypes);
    }
}
