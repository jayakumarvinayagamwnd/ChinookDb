using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record GetPlaylistByIdQuery(int PlaylistId) : IResultQuery<PlaylistDto>, ICacheableQuery
{
    public string CacheKey => $"playlists:{PlaylistId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetPlaylistByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetPlaylistByIdQueryHandler> logger) : IResultQueryHandler<GetPlaylistByIdQuery, PlaylistDto>
{
    public async Task<Result<PlaylistDto>> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetPlaylistByIdQueryHandler.Handle] - Handling GetPlaylistByIdQuery for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlist = await dbContext.Playlists
            .AsNoTracking()
            .Where(p => p.PlaylistId == request.PlaylistId)
            .ProjectTo<PlaylistDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            logger.LogWarning("[GetPlaylistByIdQueryHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        logger.LogInformation("[GetPlaylistByIdQueryHandler.Handle] - Successfully retrieved playlist for PlaylistId: {PlaylistId}", request.PlaylistId);
        return Result.Ok(playlist);
    }
}
