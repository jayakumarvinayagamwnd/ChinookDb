using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record ListPlaylistsQuery : IResultQuery<List<PlaylistDto>>, ICacheableQuery
{
    public string CacheKey => "playlists:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListPlaylistsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListPlaylistsQueryHandler> logger) : IResultQueryHandler<ListPlaylistsQuery, List<PlaylistDto>>
{
    public async Task<Result<List<PlaylistDto>>> Handle(ListPlaylistsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListPlaylistsQueryHandler.Handle] - Handling ListPlaylistsQuery");

        var playlists = await dbContext.Playlists
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ProjectTo<PlaylistDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListPlaylistsQueryHandler.Handle] - Successfully retrieved {PlaylistCount} playlists", playlists.Count);
        return Result.Ok(playlists);
    }
}
