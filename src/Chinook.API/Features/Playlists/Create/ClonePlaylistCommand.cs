using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Playlists;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Playlists;

public sealed record ClonePlaylistRequest(string? Name);

public sealed record ClonePlaylistCommand(int PlaylistId, string? Name) : IResultCommand<PlaylistDto>;

public sealed record ClonePlaylistCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ClonePlaylistCommandHandler> logger) : IResultCommandHandler<ClonePlaylistCommand, PlaylistDto>
{
    public async Task<Result<PlaylistDto>> Handle(ClonePlaylistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ClonePlaylistCommandHandler.Handle] - Handling ClonePlaylistCommand for PlaylistId: {PlaylistId}", request.PlaylistId);

        var source = await dbContext.Playlists
            .Include(p => p.PlaylistTracks)
            .Where(p => p.PlaylistId == request.PlaylistId)
            .SingleOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            logger.LogWarning("[ClonePlaylistCommandHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var newName = string.IsNullOrWhiteSpace(request.Name)
            ? $"{source.Name} (Copy)"
            : request.Name;

        var clone = new Playlist { Name = newName };
        dbContext.Playlists.Add(clone);
        await dbContext.SaveChangesAsync(cancellationToken);

        var clonedTracks = source.PlaylistTracks
            .Select(pt => new PlaylistTrack { PlaylistId = clone.PlaylistId, TrackId = pt.TrackId })
            .ToList();

        dbContext.PlaylistTracks.AddRange(clonedTracks);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[ClonePlaylistCommandHandler.Handle] - Successfully cloned PlaylistId: {SourceId} to new PlaylistId: {NewId}", request.PlaylistId, clone.PlaylistId);
        return Result.Ok(mapper.Map<PlaylistDto>(clone));
    }
}
