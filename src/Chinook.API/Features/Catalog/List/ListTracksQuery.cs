using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ListTracksQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<TrackDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"catalog:tracks:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListTracksQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListTracksQueryHandler> logger) : IResultQueryHandler<ListTracksQuery, OffsetPagedResponse<TrackDto>>
{
    public async Task<Result<OffsetPagedResponse<TrackDto>>> Handle(ListTracksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListTracksQueryHandler.Handle] - Handling ListTracksQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .OrderBy(track => track.Name)
            .ThenBy(track => track.TrackId)
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListTracksQueryHandler.Handle] - Successfully retrieved {TrackCount} tracks out of {TotalCount}", tracks.Items.Count, tracks.TotalCount);
        return Result.Ok(tracks);
    }
}
