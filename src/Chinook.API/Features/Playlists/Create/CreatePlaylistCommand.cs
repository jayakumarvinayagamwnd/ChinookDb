using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Playlists;
using FluentResults;

namespace Chinook.API.Features.Playlists;

public sealed record CreatePlaylistCommand(string Name) : IResultCommand<PlaylistDto>;

public sealed record CreatePlaylistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreatePlaylistCommandHandler> logger) : IResultCommandHandler<CreatePlaylistCommand, PlaylistDto>
{
    public async Task<Result<PlaylistDto>> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreatePlaylistCommandHandler.Handle] - Handling CreatePlaylistCommand for Name: {Name}", request.Name);

        var playlist = mapper.Map<Playlist>(request);
        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreatePlaylistCommandHandler.Handle] - Successfully created playlist with PlaylistId: {PlaylistId}", playlist.PlaylistId);
        return Result.Ok(mapper.Map<PlaylistDto>(playlist));
    }
}
