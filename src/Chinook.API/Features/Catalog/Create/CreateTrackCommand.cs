using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record CreateTrackCommand(
    string Name,
    int? AlbumId,
    int MediaTypeId,
    int? GenreId,
    string? Composer,
    int Milliseconds,
    int? Bytes,
    decimal UnitPrice) : IResultCommand<TrackDto>;

public sealed record CreateTrackCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateTrackCommandHandler> logger) : IResultCommandHandler<CreateTrackCommand, TrackDto>
{
    public async Task<Result<TrackDto>> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateTrackCommandHandler.Handle] - Handling CreateTrackCommand for Name: {Name}", request.Name);

        var validationResult = await ValidateReferencesAsync(dbContext, request.AlbumId, request.MediaTypeId, request.GenreId, cancellationToken);
        if (validationResult is not null)
            return validationResult;

        var track = mapper.Map<Track>(request);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateTrackCommandHandler.Handle] - Successfully created track with TrackId: {TrackId}", track.TrackId);
        return Result.Ok(mapper.Map<TrackDto>(track));
    }

    private static async Task<Result<TrackDto>?> ValidateReferencesAsync(ChinookDbContext dbContext, int? albumId, int mediaTypeId, int? genreId, CancellationToken cancellationToken)
    {
        if (albumId.HasValue)
        {
            var albumExists = await dbContext.Albums.AsNoTracking().AnyAsync(a => a.AlbumId == albumId.Value, cancellationToken);
            if (!albumExists)
                return Result.Fail($"Album with ID {albumId.Value} not found.");
        }

        var mediaTypeExists = await dbContext.MediaTypes.AsNoTracking().AnyAsync(m => m.MediaTypeId == mediaTypeId, cancellationToken);
        if (!mediaTypeExists)
            return Result.Fail($"Media type with ID {mediaTypeId} not found.");

        if (genreId.HasValue)
        {
            var genreExists = await dbContext.Genres.AsNoTracking().AnyAsync(g => g.GenreId == genreId.Value, cancellationToken);
            if (!genreExists)
                return Result.Fail($"Genre with ID {genreId.Value} not found.");
        }

        return null;
    }
}
