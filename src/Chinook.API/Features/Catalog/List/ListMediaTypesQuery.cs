using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListMediaTypesQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<MediaTypeDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"catalog:media-types:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public sealed record ListMediaTypesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListMediaTypesQueryHandler> logger) : IResultQueryHandler<ListMediaTypesQuery, OffsetPagedResponse<MediaTypeDto>>
{
    public async Task<Result<OffsetPagedResponse<MediaTypeDto>>> Handle(ListMediaTypesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListMediaTypesQueryHandler.Handle] - Handling ListMediaTypesQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var mediaTypes = await dbContext.MediaTypes
            .AsNoTracking()
            .OrderBy(mediaType => mediaType.Name)
            .ThenBy(mediaType => mediaType.MediaTypeId)
            .ProjectTo<MediaTypeDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListMediaTypesQueryHandler.Handle] - Successfully retrieved {MediaTypeCount} media types out of {TotalCount}", mediaTypes.Items.Count, mediaTypes.TotalCount);
        return Result.Ok(mediaTypes);
    }
}
