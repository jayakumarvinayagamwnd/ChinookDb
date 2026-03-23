using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record ListPlaylistsQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<PlaylistDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"playlists:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListPlaylistsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListPlaylistsQueryHandler> logger) : IResultQueryHandler<ListPlaylistsQuery, OffsetPagedResponse<PlaylistDto>>
{
    public async Task<Result<OffsetPagedResponse<PlaylistDto>>> Handle(ListPlaylistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListPlaylistsQueryHandler.Handle] - Handling ListPlaylistsQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var playlists = await dbContext.Playlists
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.PlaylistId)
            .ProjectTo<PlaylistDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListPlaylistsQueryHandler.Handle] - Successfully retrieved {PlaylistCount} playlists out of {TotalCount}", playlists.Items.Count, playlists.TotalCount);
        return Result.Ok(playlists);
    }
}
