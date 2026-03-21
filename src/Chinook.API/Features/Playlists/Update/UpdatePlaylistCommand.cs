using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record UpdatePlaylistRequest(string Name);

public sealed record UpdatePlaylistCommand(int PlaylistId, string Name) : IResultCommand<PlaylistDto>;

public sealed record UpdatePlaylistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdatePlaylistCommandHandler> logger) : IResultCommandHandler<UpdatePlaylistCommand, PlaylistDto>
{
    public async Task<Result<PlaylistDto>> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdatePlaylistCommandHandler.Handle] - Handling UpdatePlaylistCommand for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlist = await dbContext.Playlists
            .Where(p => p.PlaylistId == request.PlaylistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            logger.LogWarning("[UpdatePlaylistCommandHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        mapper.Map(request, playlist);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdatePlaylistCommandHandler.Handle] - Successfully updated playlist with PlaylistId: {PlaylistId}", playlist.PlaylistId);
        return Result.Ok(mapper.Map<PlaylistDto>(playlist));
    }
}
