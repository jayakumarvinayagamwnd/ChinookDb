using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record UpdateTrackRequest(
    string Name,
    int? AlbumId,
    int MediaTypeId,
    int? GenreId,
    string? Composer,
    int Milliseconds,
    int? Bytes,
    decimal UnitPrice);

public sealed record UpdateTrackCommand(
    int TrackId,
    string Name,
    int? AlbumId,
    int MediaTypeId,
    int? GenreId,
    string? Composer,
    int Milliseconds,
    int? Bytes,
    decimal UnitPrice) : IResultCommand<TrackDto>;

public sealed record UpdateTrackCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateTrackCommandHandler> logger) : IResultCommandHandler<UpdateTrackCommand, TrackDto>
{
    public async Task<Result<TrackDto>> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateTrackCommandHandler.Handle] - Handling UpdateTrackCommand for TrackId: {TrackId}", request.TrackId);

        var track = await dbContext.Tracks
            .Where(t => t.TrackId == request.TrackId)
            .SingleOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            logger.LogWarning("[UpdateTrackCommandHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        var validationResult = await ValidateReferencesAsync(dbContext, request.AlbumId, request.MediaTypeId, request.GenreId, cancellationToken);
        if (validationResult is not null)
            return validationResult;

        mapper.Map(request, track);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateTrackCommandHandler.Handle] - Successfully updated track with TrackId: {TrackId}", track.TrackId);
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
